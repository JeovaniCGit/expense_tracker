using ErrorOr;
using ExpenseTracker.Application.Abstractions.DbExceptionHandler;
using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Application.Categories.Errors;
using ExpenseTracker.Application.Collections.Errors;
using ExpenseTracker.Application.Records.Contracts.Requests;
using ExpenseTracker.Application.Records.Contracts.Responses;
using ExpenseTracker.Application.Records.Errors;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Domain.Categories.Repository;
using ExpenseTracker.Domain.Collection.Repository;
using ExpenseTracker.Domain.Collections.Entity;
using ExpenseTracker.Domain.Records.Entity;
using ExpenseTracker.Domain.Records.Repository;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Records.Services;

public sealed class TransactionRecordService : ITransactionRecordService
{
    private readonly ITransactionRecordRepository _transactionRecordRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITransactionRecordCategoryRepository _transactionRecordCategoryRepository;
    private readonly ITransactionCollectionRepository _transactionCollectionRepository;
    private readonly IValidator<AddTransactionRecordRequestDto> _addRecordValidator;
    private readonly IValidator<UpdateTransactionRecordRequestDto> _updateRecordValidator;
    private readonly IValidator<List<UpdateTransactionRecordRequestDto>> _updateRecordsValidator;
    private readonly ICurrentUserService _currentUserService;

    public TransactionRecordService(
        ITransactionRecordRepository transactionRecordRepository,
        IUserRepository userRepository,
        ITransactionRecordCategoryRepository transactionRecordCategoryRepository,
        ITransactionCollectionRepository transactionCollectionRepository,
        IValidator<AddTransactionRecordRequestDto> addRecordvalidator,
        IValidator<UpdateTransactionRecordRequestDto> updateRecordValidator,
        IValidator<List<UpdateTransactionRecordRequestDto>> updateRecordsValidator,
        ICurrentUserService currentUserService
        )
    {
        _transactionRecordRepository = transactionRecordRepository;
        _userRepository = userRepository;
        _transactionRecordCategoryRepository = transactionRecordCategoryRepository;
        _transactionCollectionRepository = transactionCollectionRepository;
        _addRecordValidator = addRecordvalidator;
        _updateRecordValidator = updateRecordValidator;
        _updateRecordsValidator = updateRecordsValidator;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<AddTransactionRecordResponseDto>> AddUserTransactionRecord(AddTransactionRecordRequestDto request, CancellationToken ctoken = default)
    {
        await _addRecordValidator.ValidateAndThrowAsync(request, ctoken);

        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(request.TransactionUserExternalId));

        if (existingUser is null)
            return TransactionRecordErrors.InvalidArgs;

        long? existingCollectionId = await _transactionCollectionRepository.GetCollectionIdByExternalId(Guid.Parse(request.TransactionCollectionExternalId), ctoken);

        if (existingCollectionId is null)
            return TransactionRecordErrors.InvalidArgs;

        IEnumerable<TransactionRecordCategory> defaultCategories = await _transactionRecordCategoryRepository.GetAllTransactionsCategories(ctoken);
        IEnumerable<TransactionRecordCategory> userCategories = await _transactionRecordCategoryRepository.GetAllUserTransactionCategories(existingUser.Id, ctoken);

        long? existingCategoryId = userCategories.Where(tc => tc.ExternalId == Guid.Parse(request.TransactionCategoryExternalId)).Select(tc => tc.Id).FirstOrDefault();

        if (existingCategoryId is null || existingCategoryId == 0)
            existingCategoryId = defaultCategories.Where(tc => tc.ExternalId == Guid.Parse(request.TransactionCategoryExternalId)).Select(tc => tc.Id).FirstOrDefault();

        if (existingCategoryId is null || existingCategoryId == 0)
            return TransactionRecordErrors.InvalidArgs;

        try
        {
            TransactionRecord mappedRecord = new TransactionRecord
            {
                TransactionValue = request.TransactionValue,
                TransactionUserId = existingUser.Id,
                TransactionCategoryId = (long)existingCategoryId,
                TransactionCollectionId = (long)existingCollectionId
            };

            TransactionRecord addedRecord = await _transactionRecordRepository.AddTransaction(mappedRecord, ctoken);

            return new AddTransactionRecordResponseDto
            {
                TransactionValue = addedRecord.TransactionValue,
                CreatedAt = addedRecord.CreatedAt,
                ExternalId = addedRecord.ExternalId
            };
        } catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            return TransactionRecordErrors.DuplicatedEntry;
        }
    }

    public async Task<ErrorOr<int>> DeleteTransactionRecord(string recordExternalId, CancellationToken ctoken = default)
    {
        Guid currrentUserExternalId = _currentUserService.UserExternalId;
        User? currentUser = await _userRepository.GetUserByExternalId(currrentUserExternalId, ctoken);

        TransactionRecord? existingRecord = await _transactionRecordRepository.GetTransactionRecordByExternalId(Guid.Parse(recordExternalId), ctoken);

        if (existingRecord is null)
            return TransactionRecordErrors.NotFound;

        if (currentUser.RoleId == (long)UserRoleEnum.Admin)
            return await _transactionRecordRepository.DeleteTransactionRecord(existingRecord, ctoken);

        if (existingRecord.TransactionUserId != currentUser.Id)
            return TransactionRecordErrors.NotOwner;

        return await _transactionRecordRepository.DeleteTransactionRecord(existingRecord, ctoken);
    }

    public async Task<ErrorOr<IEnumerable<GetTransactionRecordResponseDto>>> GetAllUserTransactionsByCategory(string categoryExternalId, CancellationToken ctoken = default)
    {
        Guid currentUserExternalId = _currentUserService.UserExternalId;

        User? currentUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);

        long? existingCategoryId = await _transactionRecordCategoryRepository.GetTransactionCategoryIdByExternalId(Guid.Parse(categoryExternalId), ctoken);

        if (existingCategoryId is null)
            return TransactionRecordCategoryErrors.NotFound;

        IEnumerable<TransactionRecord> records = await _transactionRecordRepository.GetAllUserTransactionsByCategory(currentUser!.Id, existingCategoryId.Value, ctoken);

        return records.Select(tr => new GetTransactionRecordResponseDto
        {
            TransactionValue = tr.TransactionValue,
            TransactionExternalId = tr.ExternalId,
            TransactionCategoryExternalId = Guid.Parse(categoryExternalId),
            TransactionCategoryName = tr.TransactionCategory.CategoryName
        }).ToList();
    }

    public async Task<ErrorOr<int>> UpdateAllUserTransactions(List<UpdateTransactionRecordRequestDto> request, CancellationToken ctoken = default)
    {
        await _updateRecordsValidator.ValidateAndThrowAsync(request, ctoken);

        Guid currentUserExternalId = _currentUserService.UserExternalId;
        User? currentUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);

        var parsedRequestData = request.Select(r => new
        {
            RecordExternalId = Guid.Parse(r.TransactionExternalId),
            CategoryExternalId = Guid.Parse(r.TransactionCategoryExternalId),
            Value = r.TransactionValue
        }).ToList();

        List<Guid> recordsExternalIds = parsedRequestData
            .Select(data => data.RecordExternalId)
            .ToList();

        IEnumerable<TransactionRecord> existingRecords = await _transactionRecordRepository.GetUserTransactionsByExternalId(currentUser!.Id, recordsExternalIds, ctoken);
        var recordsLookup = existingRecords.ToDictionary(r => r.ExternalId);

        var missingRecords = parsedRequestData
            .Where(r => !recordsLookup.ContainsKey(r.RecordExternalId))
            .ToList();

        if (missingRecords.Any())
            return TransactionRecordErrors.InvalidArgs;


        // Categories validation
        List<Guid> requestCategoryExternalIds = parsedRequestData
            .Select(r => r.CategoryExternalId)
            .ToList();

        IEnumerable<TransactionRecordCategory> existingCategories = await _transactionRecordCategoryRepository.GetUserCategoriesByExternalIds(currentUser.Id, requestCategoryExternalIds, ctoken);

        var categoryLookup = existingCategories.ToDictionary(c => c.ExternalId, c => c.Id);

        if (requestCategoryExternalIds.Any(externalId => !categoryLookup.ContainsKey(externalId)))
            return TransactionRecordErrors.InvalidArgs;


        // Update
        try
        {
            List<TransactionRecord> updatedRecords = parsedRequestData.Select(data =>
            {
                TransactionRecord record = recordsLookup[data.RecordExternalId];
                record.TransactionValue = data.Value;
                return record;
            }).ToList();

            return await _transactionRecordRepository.SaveChanges(ctoken);

        }
        catch (DbUpdateConcurrencyException ex)
        {
            return TransactionRecordErrors.ConcurrencyConflict;
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            return TransactionRecordErrors.DuplicatedEntry;
        }
    }

    public async Task<ErrorOr<int>> UpdateUserTransaction(UpdateTransactionRecordRequestDto request, CancellationToken ctoken = default)
    {
        await _updateRecordValidator.ValidateAndThrowAsync(request, ctoken);

        Guid currentUserExternalId = _currentUserService.UserExternalId;
        User? currentUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);

        TransactionRecord? existingRecord = await _transactionRecordRepository.GetUserTransactionByCategoryExternalId(Guid.Parse(request.TransactionExternalId), Guid.Parse(request.TransactionCategoryExternalId), ctoken);

        if (existingRecord is null)
            return TransactionRecordErrors.InvalidArgs;

        if (existingRecord.TransactionUserId != currentUser!.Id)
            return TransactionRecordErrors.NotOwner;

        existingRecord.TransactionValue = request.TransactionValue;

        try
        {
            return await _transactionRecordRepository.SaveChanges(ctoken);

        }
        catch (DbUpdateConcurrencyException ex)
        {
            return TransactionRecordErrors.ConcurrencyConflict;
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            return TransactionRecordErrors.DuplicatedEntry; 
        }
    }

    public async Task<ErrorOr<IEnumerable<GetTransactionRecordResponseDto>>> GetAllTransactionsByCollectionId(string collectionExternalId, CancellationToken ctoken = default)
    {
        Guid currentUserExternalId = _currentUserService.UserExternalId;

        User? currentUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);

        TransactionCollection? collectionExists = await _transactionCollectionRepository.GetUserCollectionByExternalId(currentUser.Id, Guid.Parse(collectionExternalId), ctoken);

        if (collectionExists is null)
            return CollectionErrors.NotFound;

        IEnumerable<TransactionRecord> records = await _transactionRecordRepository.GetAllUserTransactionsByCollection(currentUser.Id, collectionExists.Id, ctoken);

        return records.Select(r => new GetTransactionRecordResponseDto
        {
            TransactionValue = r.TransactionValue,
            TransactionExternalId = r.ExternalId,
            TransactionCategoryExternalId = r.TransactionCategory.ExternalId,
            TransactionCategoryName = r.TransactionCategory.CategoryName
        }).ToList();
    }
}