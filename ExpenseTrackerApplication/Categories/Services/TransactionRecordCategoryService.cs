using ErrorOr;
using ExpenseTracker.Application.Accounts.Errors;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Application.Categories.Contracts.Requests;
using ExpenseTracker.Application.Categories.Contracts.Responses;
using ExpenseTracker.Application.Categories.Errors;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Domain.Categories.Repository;
using FluentValidation;

namespace ExpenseTracker.Application.Categories.Services;

public sealed class TransactionRecordCategoryService : ITransactionRecordCategoryService
{
    private readonly ITransactionRecordCategoryRepository _transactionRecordCategoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IValidator<AddTransactionRecordCategoryRequestDto> _addCategoryValidator;
    private readonly IValidator<UpdateTransactionRecordCategoryRequestDto> _updateCategoryValidator;
    private readonly IValidator<List<UpdateTransactionRecordCategoryRequestDto>> _updateCategoriesValidator;

    public TransactionRecordCategoryService(
        ITransactionRecordCategoryRepository transactionRecordCategoryRepository,
        IUserRepository userRepository,
        IValidator<AddTransactionRecordCategoryRequestDto> addCategoryValidator,
        IValidator<UpdateTransactionRecordCategoryRequestDto> updateCategoryValidator,
        IValidator<List<UpdateTransactionRecordCategoryRequestDto>> updateCategoriesValidator
        )
    {
        _transactionRecordCategoryRepository = transactionRecordCategoryRepository;
        _userRepository = userRepository;
        _addCategoryValidator = addCategoryValidator;
        _updateCategoryValidator = updateCategoryValidator;
        _updateCategoriesValidator = updateCategoriesValidator;
    }

    public async Task<ErrorOr<AddTransactionRecordCategoryResponseDto>> AddUserTransactionRecordCategory(AddTransactionRecordCategoryRequestDto request, CancellationToken ctoken = default)
    {
        await _addCategoryValidator.ValidateAndThrowAsync(request, ctoken);

        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(request.UserExternalId), ctoken);
        if (existingUser is null)
            return TransactionRecordCategoryErrors.InvalidArgs;

        TransactionRecordCategory newCategory = new TransactionRecordCategory
        {
            CategoryName = request.CategoryName,
            UserId = existingUser.Id
        };
        TransactionRecordCategory addedCategory = await _transactionRecordCategoryRepository.AddTransactionCategory(newCategory);

        return new AddTransactionRecordCategoryResponseDto
        {
            CategoryExternalId = addedCategory.ExternalId,
            CategoryName = addedCategory.CategoryName,
            TransactionCategoryUserExternalId = addedCategory.ExternalId,
            CreatedAt = addedCategory.CreatedAt,
        };
    }

    public async Task<ErrorOr<int>> DeleteTransactionRecordCategory(string userExternalId, string categoryExternalId, CancellationToken ctoken = default)
    {
        TransactionRecordCategory? existingCategory = await _transactionRecordCategoryRepository.GetTransactionsCategoryByExternalId(Guid.Parse(categoryExternalId), ctoken);
        if (existingCategory is null)
            return TransactionRecordCategoryErrors.NotFound;

        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(userExternalId), ctoken);
        if (existingUser is null)
            return UserErrors.NotFound;

        if (existingUser.RoleId == (long)UserRoleEnum.Admin)
            return await _transactionRecordCategoryRepository.DeleteTransactionCategory(existingCategory, ctoken);

        if ((long)existingCategory.UserId != (long)existingUser.Id)
            return TransactionRecordCategoryErrors.NotOwner;

        return await _transactionRecordCategoryRepository.DeleteTransactionCategory(existingCategory, ctoken);
    }

    public async Task<ErrorOr<IEnumerable<GetTransactionRecordCategoryResponseDto>>> GetAllUserTransactionCategories(string userExternalId, CancellationToken ctoken = default)
    {
        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(userExternalId));
        if (existingUser is null)
            return TransactionRecordCategoryErrors.InvalidArgs;

        IEnumerable<TransactionRecordCategory> userCategories = await _transactionRecordCategoryRepository.GetAllUserTransactionCategories(existingUser.Id, ctoken);

        return userCategories.Select(uc => new GetTransactionRecordCategoryResponseDto
        {
            CategoryName = uc.CategoryName,
            CategoryExternalId = uc.ExternalId
        }).ToList();
    }

    public async Task<ErrorOr<int>> UpdateAllUserTransactionCategories(string userExternalId, List<UpdateTransactionRecordCategoryRequestDto> request, CancellationToken ctoken = default)
    {
        await _updateCategoriesValidator.ValidateAndThrowAsync(request, ctoken);

        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(userExternalId));
        if (existingUser is null)
            return TransactionRecordCategoryErrors.InvalidArgs;

        List<Guid> externalIdsFromRequest = request.Select(c => Guid.Parse(c.CategoryExternalId)).Distinct().ToList();
        IEnumerable<TransactionRecordCategory> existingCategories = await _transactionRecordCategoryRepository.GetAllUserTransactionCategories(existingUser.Id, ctoken);
        Dictionary<Guid, long> categoryLookup = existingCategories.ToDictionary(c => c.ExternalId, c => c.Id);

        if (externalIdsFromRequest.All(id => categoryLookup.ContainsKey(id)))
            return TransactionRecordCategoryErrors.NotOwner;

        List<TransactionRecordCategory> mappedCategories = request.Select(c =>
        {
            Guid categoryExternalId = Guid.Parse(c.CategoryExternalId);

            return new TransactionRecordCategory
            {
                CategoryName = c.CategoryName,
                UserId = existingUser.Id,
            };
        }).ToList();

        return await _transactionRecordCategoryRepository.UpdateAllUserTransactionCategories(mappedCategories, ctoken);
    }

    public async Task<ErrorOr<int>> UpdateUserTransactionCategory(string userExternalId, UpdateTransactionRecordCategoryRequestDto request, CancellationToken ctoken = default)
    {
        await _updateCategoryValidator.ValidateAndThrowAsync(request, ctoken);

        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(userExternalId));
        if (existingUser is null)
            return TransactionRecordCategoryErrors.InvalidArgs;

        Guid externalIdFromRequest = Guid.Parse(request.CategoryExternalId);

        long? existingCategoryId = await _transactionRecordCategoryRepository.GetTransactionCategoryIdByExternalId(externalIdFromRequest, ctoken);
        if (existingCategoryId is null)
            return TransactionRecordCategoryErrors.NotFound;

        TransactionRecordCategory? existingCategory = await _transactionRecordCategoryRepository.GetTransactionRecordCategoryById((long)existingCategoryId, ctoken);
        if (existingCategory!.UserId != existingUser.Id)
            return TransactionRecordCategoryErrors.NotOwner;

        TransactionRecordCategory mappedCategory = new TransactionRecordCategory
        {
            CategoryName = request.CategoryName,
            UserId = existingUser.Id,
        };

        return await _transactionRecordCategoryRepository.UpdateUserTransactionCategory(mappedCategory, ctoken);
    }
}
