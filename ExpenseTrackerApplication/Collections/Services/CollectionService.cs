using ErrorOr;
using ExpenseTracker.Application.Collections.Contracts.Requests;
using ExpenseTracker.Application.Collections.Contracts.Responses;
using ExpenseTracker.Application.Collections.Errors;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Collection.Repository;
using ExpenseTracker.Domain.Collections.Entity;
using FluentValidation;

namespace ExpenseTracker.Application.Collections.Services;
public sealed class CollectionService : ICollectionService
{
    private readonly ITransactionCollectionRepository _transactionCollectionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IValidator<AddCollectionRequestDto> _addCollectionValidator;
    private readonly IValidator<UpdateCollectionRequestDto> _updateCollectionValidator;

    public CollectionService(
        ITransactionCollectionRepository transactionCollectionRepository, 
        IUserRepository userRepository,
        IValidator<AddCollectionRequestDto> addCollectionValidator,
        IValidator<UpdateCollectionRequestDto> updateCollectionValidator
        )
    {
        _transactionCollectionRepository = transactionCollectionRepository;
        _userRepository = userRepository;
        _addCollectionValidator = addCollectionValidator;
        _updateCollectionValidator = updateCollectionValidator;
    }

    public async Task<ErrorOr<AddCollectionResponseDto>> AddCollection(AddCollectionRequestDto request, CancellationToken ctoken = default)
    {
        await _addCollectionValidator.ValidateAndThrowAsync(request, ctoken);

        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(request.UserExternalId), ctoken);
        if (existingUser is null)
            return CollectionErrors.Unauthorized;

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
    }

    public async Task<ErrorOr<int>> DeleteCollection(string collectionExternalId, CancellationToken ctoken = default)
    {
        TransactionCollection? existingCollection = await _transactionCollectionRepository.GetCollectionByExternalId(Guid.Parse(collectionExternalId), ctoken);

        if (existingCollection is null)
            return CollectionErrors.NotFound;

        return await _transactionCollectionRepository.DeleteCollection(existingCollection, ctoken);
    }

    public async Task<ErrorOr<IEnumerable<GetCollectionResponseDto>>> GetAllUserCollections(string userExternalId, DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken ctoken = default)
    {
        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(userExternalId), ctoken);
        if (existingUser is null)
            return CollectionErrors.Unauthorized;

        IEnumerable<TransactionCollection> collectionMapped = await _transactionCollectionRepository.GetAllUserCollections(existingUser.Id, startDate?? null, endDate?? null, ctoken);

        return collectionMapped.Select(c => new GetCollectionResponseDto
        {
            Description = c.Description,
            CollectionExternalId = c.ExternalId,
        }).ToList();
    }

    public async Task<ErrorOr<int>> UpdateCollection(UpdateCollectionRequestDto request, CancellationToken ctoken = default)
    {
        await _updateCollectionValidator.ValidateAndThrowAsync(request, ctoken);

        TransactionCollection? existingCollection = await _transactionCollectionRepository.GetCollectionByExternalId(Guid.Parse(request.CollectionExternalId), ctoken);
        if (existingCollection is null)
            return CollectionErrors.NotFound;

        TransactionCollection collectionMapped = new TransactionCollection
        {
            Description = request.Description ?? existingCollection.Description,
            UserId = existingCollection.UserId,
            EstimatedBudget = request.EstimatedBudget ?? existingCollection.EstimatedBudget,
            RealBudget = request.RealBudget ?? existingCollection.RealBudget,
            StartDate = request.StartDate ?? existingCollection.StartDate,
            EndDate = request.EndDate ?? existingCollection.EndDate
        };

        return await _transactionCollectionRepository.UpdateCollection(collectionMapped, ctoken);
    }
}
