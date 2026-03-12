using ErrorOr;
using ExpenseTracker.Application.Abstractions.DbExceptionHandler;
using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Collections.Contracts.Requests;
using ExpenseTracker.Application.Collections.Contracts.Responses;
using ExpenseTracker.Application.Collections.Errors;
using ExpenseTracker.Application.Records.Errors;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Collection.Repository;
using ExpenseTracker.Domain.Collections.Entity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Collections.Services;
public sealed class CollectionService : ICollectionService
{
    private readonly ITransactionCollectionRepository _transactionCollectionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IValidator<AddCollectionRequestDto> _addCollectionValidator;
    private readonly IValidator<UpdateCollectionRequestDto> _updateCollectionValidator;
    private readonly ICurrentUserService _currentUserService;

    public CollectionService(
        ITransactionCollectionRepository transactionCollectionRepository, 
        IUserRepository userRepository,
        IValidator<AddCollectionRequestDto> addCollectionValidator,
        IValidator<UpdateCollectionRequestDto> updateCollectionValidator,
        ICurrentUserService currentUserService
        )
    {
        _transactionCollectionRepository = transactionCollectionRepository;
        _userRepository = userRepository;
        _addCollectionValidator = addCollectionValidator;
        _updateCollectionValidator = updateCollectionValidator;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<AddCollectionResponseDto>> AddCollection(AddCollectionRequestDto request, CancellationToken ctoken = default)
    {
        await _addCollectionValidator.ValidateAndThrowAsync(request, ctoken);

        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(request.UserExternalId), ctoken);
        if (existingUser is null)
            return CollectionErrors.InvalidArgs;

        try
        {
            TransactionCollection collectionMapped = new TransactionCollection
            {
                Description = request.Description,
                UserId = existingUser.Id,
                EstimatedBudget = request.EstimatedBudget,
                RealBudget = request.RealBudget,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            TransactionCollection newAddedCollection = await _transactionCollectionRepository.AddCollection(collectionMapped, ctoken);

            return new AddCollectionResponseDto
            {
                ExternalId = newAddedCollection.ExternalId,
                Description = newAddedCollection.Description,
                CreatedAt = newAddedCollection.CreatedAt
            };
        } catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            return CollectionErrors.DuplicatedEntry;
        }
    }

    public async Task<ErrorOr<int>> DeleteCollection(string collectionExternalId, CancellationToken ctoken = default)
    {
        Guid currentUserExternalId = _currentUserService.UserExternalId;

        User? currentUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);

        TransactionCollection? existingCollection = await _transactionCollectionRepository.GetCollectionByExternalId(Guid.Parse(collectionExternalId), ctoken);

        if (existingCollection is null)
            return CollectionErrors.NotFound;

        if (currentUser.Id != existingCollection.UserId)
            return CollectionErrors.NotOwner;

        return await _transactionCollectionRepository.DeleteCollection(existingCollection, ctoken);
    }

    public async Task<ErrorOr<IEnumerable<GetCollectionResponseDto>>> GetAllUserCollections(DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken ctoken = default)
    {
        Guid currentUserExternalId = _currentUserService.UserExternalId;

        User? currentUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);

        IEnumerable<TransactionCollection> collections = await _transactionCollectionRepository.GetAllUserCollections(currentUser.Id, startDate?? null, endDate?? null, ctoken);

        return collections.Select(c => new GetCollectionResponseDto
        {
            Description = c.Description,
            CollectionExternalId = c.ExternalId,
            EstimatedBudget = c.EstimatedBudget,
            RealBudget = c.RealBudget,
            StartDate = c.StartDate,
            EndDate = c.EndDate

        }).ToList();
    }

    public async Task<ErrorOr<int>> UpdateCollection(UpdateCollectionRequestDto request, CancellationToken ctoken = default)
    {
        await _updateCollectionValidator.ValidateAndThrowAsync(request, ctoken);

        Guid currentUserExternalId = _currentUserService.UserExternalId;

        User? currentUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);

        TransactionCollection? existingCollection = await _transactionCollectionRepository.GetCollectionByExternalId(Guid.Parse(request.CollectionExternalId), ctoken);

        if (existingCollection is null)
            return CollectionErrors.InvalidArgs;

        if (currentUser!.Id != existingCollection.UserId)
            return CollectionErrors.NotOwner;

        existingCollection.Description = request.Description ?? existingCollection.Description;
        existingCollection.EstimatedBudget = request.EstimatedBudget ?? existingCollection.EstimatedBudget;
        existingCollection.RealBudget = request.RealBudget ?? existingCollection.RealBudget;
        existingCollection.StartDate = request.StartDate ?? existingCollection.StartDate;
        existingCollection.EndDate = request.EndDate ?? existingCollection.EndDate;

        try
        {
            return await _transactionCollectionRepository.SaveChanges(ctoken);

        }
        catch (DbUpdateConcurrencyException ex)
        {
            return CollectionErrors.ConcurrencyConflict;
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            return CollectionErrors.DuplicatedEntry;
        }
    }
}