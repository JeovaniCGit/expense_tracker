using ErrorOr;
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
using ExpenseTracker.Domain.Records.Entity;
using ExpenseTracker.Domain.Records.Repository;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace ExpenseTracker.Application.Records.Services;

public sealed class TransactionRecordService : ITransactionRecordService
{
    private readonly ITransactionRecordRepository _transactionRecordRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITransactionRecordCategoryRepository _transactionRecordCategoryRepository;
    private readonly ITransactionCollectionRepository _transactionCollectionRepository;
    private readonly IHttpContextAccessor _context;
    private readonly IValidator<AddTransactionRecordRequestDto> _addRecordValidator;
    private readonly IValidator<UpdateTransactionRecordRequestDto> _updateRecordValidator;
    private readonly IValidator<List<UpdateTransactionRecordRequestDto>> _updateRecordsValidator;

    public TransactionRecordService(
        ITransactionRecordRepository transactionRecordRepository,
        IUserRepository userRepository,
        ITransactionRecordCategoryRepository transactionRecordCategoryRepository,
        ITransactionCollectionRepository transactionCollectionRepository,
        IHttpContextAccessor context,
        IValidator<AddTransactionRecordRequestDto> addRecordvalidator,
        IValidator<UpdateTransactionRecordRequestDto> updateRecordValidator,
        IValidator<List<UpdateTransactionRecordRequestDto>> updateRecordsValidator
        )
    {
        _transactionRecordRepository = transactionRecordRepository;
        _userRepository = userRepository;
        _transactionRecordCategoryRepository = transactionRecordCategoryRepository;
        _transactionCollectionRepository = transactionCollectionRepository;
        _context = context;
        _addRecordValidator = addRecordvalidator;
        _updateRecordValidator = updateRecordValidator;
        _updateRecordsValidator = updateRecordsValidator;
    }

    public async Task<ErrorOr<AddTransactionRecordResponseDto>> AddUserTransactionRecord(AddTransactionRecordRequestDto request, CancellationToken ctoken = default)
    {
        await _addRecordValidator.ValidateAndThrowAsync(request, ctoken);

        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(request.TransactionUserExternalId));
        if (existingUser is null)
            return TransactionRecordErrors.Unauthorized;

        if (request.TransactionValue < 0)
            return TransactionRecordErrors.InvalidArgs;

        IEnumerable<TransactionRecordCategory> defaultCategories = await _transactionRecordCategoryRepository.GetAllTransactionsCategories(ctoken);
        IEnumerable<TransactionRecordCategory> userCategories = await _transactionRecordCategoryRepository.GetAllUserTransactionCategories(existingUser.Id, ctoken);

        TransactionRecordCategory? validatedCategoryId = userCategories.Where(tc => tc.ExternalId == Guid.Parse(request.TransactionCategoryExternalId)).FirstOrDefault();
        if (validatedCategoryId is null)
        {
            validatedCategoryId = defaultCategories.Where(tc => tc.ExternalId == Guid.Parse(request.TransactionCategoryExternalId)).FirstOrDefault();
        }

        if (validatedCategoryId is null)
            return TransactionRecordCategoryErrors.NotFound;

        TransactionRecord mappedRecord = new TransactionRecord
        {
            TransactionValue = request.TransactionValue,
            TransactionUserId = existingUser.Id,
            TransactionCategoryId = validatedCategoryId.Id
        };

        TransactionRecord addedRecord = await _transactionRecordRepository.AddTransaction(mappedRecord, ctoken);

        return new AddTransactionRecordResponseDto
        {
            TransactionValue = addedRecord.TransactionValue,
            CreatedAt = addedRecord.CreatedAt,
            ExternalId = addedRecord.ExternalId
        };
    }

    public async Task<ErrorOr<int>> DeleteTransactionRecord(string userExternalId, string recordExternalId, CancellationToken ctoken = default)
    {
        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(userExternalId), ctoken);
        if (existingUser is null)
            return TransactionRecordErrors.Unauthorized;

        TransactionRecord? existingRecord = await _transactionRecordRepository.GetTransactionRecordByExternalId(Guid.Parse(recordExternalId), ctoken);
        if (existingRecord is null)
            return TransactionRecordErrors.NotFound;

        if (existingUser.Role.Id == (long)UserRoleEnum.Admin)
            return await _transactionRecordRepository.DeleteTransactionRecord(existingRecord, ctoken);


        bool isUserOwner = existingRecord.TransactionUserId == existingUser.Id;
        if (!isUserOwner)
            return TransactionRecordErrors.NotOwner;

        return await _transactionRecordRepository.DeleteTransactionRecord(existingRecord, ctoken);
    }

    public async Task<ErrorOr<IEnumerable<GetTransactionRecordResponseDto>>> GetAllUserTransactionsByCategory(string userExternalId, string categoryExternalId, CancellationToken ctoken = default)
    {
        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(userExternalId), ctoken);
        if (existingUser is null)
            return TransactionRecordErrors.Unauthorized;

        long? existingCategoryId = await _transactionRecordCategoryRepository.GetTransactionCategoryIdByExternalId(Guid.Parse(categoryExternalId), ctoken);
        if (existingCategoryId is null)
            return TransactionRecordCategoryErrors.NotFound;

        IEnumerable<TransactionRecord> records = await _transactionRecordRepository.GetAllUserTransactionsByCategory(existingUser.Id, existingCategoryId.Value);

        return records.Select(tr => new GetTransactionRecordResponseDto
        {
            TransactionValue = tr.TransactionValue,
            TransactionExternalId = tr.ExternalId,
            TransactionCategoryExternalId = Guid.Parse(categoryExternalId),
            TransactionCategoryName = tr.TransactionCategory.CategoryName
        }).ToList();
    }

    public async Task<ErrorOr<int>> UpdateAllUserTransactions(string userExternalId, List<UpdateTransactionRecordRequestDto> request, CancellationToken ctoken = default)
    {
        await _updateRecordsValidator.ValidateAndThrowAsync(request, ctoken);

        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(userExternalId));
        if (existingUser is null)
            return TransactionRecordErrors.Unauthorized;

        List<Guid> externalIdsFromRequest = request.Select(r => Guid.Parse(r.TransactionCategoryExternalId)).Distinct().ToList();
        IEnumerable<TransactionRecordCategory> existingCategories = await _transactionRecordCategoryRepository.GetAllUserTransactionCategories(existingUser.Id, ctoken);
        Dictionary<Guid, long> categoryLookup = existingCategories.ToDictionary(c => c.ExternalId, c => c.Id);

        if (externalIdsFromRequest.Any(id => !categoryLookup.ContainsKey(id)))
            return TransactionRecordErrors.NotOwner;

        List<TransactionRecord> mappedRecords = request.Select(r =>
        {
            Guid categoryExternalId = Guid.Parse(r.TransactionCategoryExternalId);

            return new TransactionRecord
            {
                TransactionValue = r.TransactionValue,
                TransactionUserId = existingUser.Id,
                TransactionCategoryId = categoryLookup[categoryExternalId]
            };
        }).ToList();

        return await _transactionRecordRepository.UpdateAllUserTransactions(mappedRecords, ctoken);
    }

    public async Task<ErrorOr<int>> UpdateUserTransaction(string userExternalId, UpdateTransactionRecordRequestDto request, CancellationToken ctoken = default)
    {
        await _updateRecordValidator.ValidateAndThrowAsync(request, ctoken);

        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(userExternalId));
        if (existingUser is null)
            return TransactionRecordErrors.Unauthorized;

        Guid externalIdFromRequest = Guid.Parse(request.TransactionExternalId);

        long? existingCategoryId = await _transactionRecordCategoryRepository.GetTransactionCategoryIdByExternalId(externalIdFromRequest, ctoken);
        if (existingCategoryId is null)
            return TransactionRecordCategoryErrors.NotFound;

        long? existingRecordId = await _transactionRecordRepository.GetTransactionRecordIdByExternalId(Guid.Parse(request.TransactionExternalId), ctoken);
        if (existingRecordId is null)
            return TransactionRecordErrors.NotFound;

        TransactionRecord? existingRecord = await _transactionRecordRepository.GetTransactionRecordById((long)existingRecordId, ctoken);
        if (existingRecord!.TransactionUserId != existingUser.Id)
            return TransactionRecordErrors.NotOwner;

        TransactionRecord mappedRecord = new TransactionRecord
        {
            TransactionUserId = existingUser.Id,
            TransactionValue = request.TransactionValue,
            TransactionCategoryId = existingRecord.TransactionCategoryId,
        };

        return await _transactionRecordRepository.UpdateTransactionRecord(mappedRecord, ctoken);
    }

    public async Task<ErrorOr<IEnumerable<GetTransactionRecordResponseDto>>> GetAllTransactionsByCollectionId(string collectionExternalId, CancellationToken ctoken = default)
    {
        long userId = Convert.ToInt32(_context.HttpContext!.User.FindFirst("sub")!.Value);

        long? collectionExistsId = await _transactionCollectionRepository.GetCollectionIdByExternalId(Guid.Parse(collectionExternalId), ctoken);
        if (collectionExistsId is null)
            return CollectionErrors.NotFound;

        IEnumerable<TransactionRecord> records = await _transactionRecordRepository.GetAllUserTransactionsByCollection(userId, (long)collectionExistsId, ctoken);

        return records.Select(r => new GetTransactionRecordResponseDto
        {
            TransactionValue = r.TransactionValue,
            TransactionExternalId = r.ExternalId,
            TransactionCategoryExternalId = r.TransactionCategory.ExternalId,
            TransactionCategoryName = r.TransactionCategory.CategoryName
        }).ToList();
    }
}
