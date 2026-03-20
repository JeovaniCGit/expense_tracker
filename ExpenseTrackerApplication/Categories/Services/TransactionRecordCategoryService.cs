using ErrorOr;
using ExpenseTracker.Application.Abstractions.DbExceptionHandler;
using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Application.Categories.Contracts.Requests;
using ExpenseTracker.Application.Categories.Contracts.Responses;
using ExpenseTracker.Application.Categories.Errors;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Domain.Categories.Repository;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Categories.Services;

public sealed class TransactionRecordCategoryService : ITransactionRecordCategoryService
{
    private readonly ITransactionRecordCategoryRepository _transactionRecordCategoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IValidator<AddTransactionRecordCategoryRequestDto> _addCategoryValidator;
    private readonly IValidator<UpdateTransactionRecordCategoryRequestDto> _updateCategoryValidator;
    private readonly IValidator<List<UpdateTransactionRecordCategoryRequestDto>> _updateCategoriesValidator;
    private readonly ICurrentUserService _currentUserService;

    public TransactionRecordCategoryService(
        ITransactionRecordCategoryRepository transactionRecordCategoryRepository,
        IUserRepository userRepository,
        IValidator<AddTransactionRecordCategoryRequestDto> addCategoryValidator,
        IValidator<UpdateTransactionRecordCategoryRequestDto> updateCategoryValidator,
        IValidator<List<UpdateTransactionRecordCategoryRequestDto>> updateCategoriesValidator,
        ICurrentUserService currentUserService
        )
    {
        _transactionRecordCategoryRepository = transactionRecordCategoryRepository;
        _userRepository = userRepository;
        _addCategoryValidator = addCategoryValidator;
        _updateCategoryValidator = updateCategoryValidator;
        _updateCategoriesValidator = updateCategoriesValidator;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<AddTransactionRecordCategoryResponseDto>> AddUserTransactionRecordCategory(AddTransactionRecordCategoryRequestDto request, CancellationToken ctoken = default)
    {
        await _addCategoryValidator.ValidateAndThrowAsync(request, ctoken);

        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(request.UserExternalId), ctoken);
        if (existingUser is null)
            return TransactionRecordCategoryErrors.InvalidArgs;

        try
        {
            TransactionRecordCategory newCategory = new TransactionRecordCategory
            {
                CategoryName = request.CategoryName,
                UserId = existingUser.Id
            };

            TransactionRecordCategory addedCategory = await _transactionRecordCategoryRepository.AddTransactionCategory(newCategory, ctoken);

            return new AddTransactionRecordCategoryResponseDto
            {
                CategoryExternalId = addedCategory.ExternalId,
                CategoryName = addedCategory.CategoryName,
                CreatedAt = addedCategory.CreatedAt,
            };
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            return TransactionRecordCategoryErrors.DuplicatedEntry;
        }
    }

    public async Task<ErrorOr<int>> DeleteTransactionRecordCategory(string categoryExternalId, CancellationToken ctoken = default)
    {
        Guid currentUserExternalId = _currentUserService.UserExternalId;

        TransactionRecordCategory? existingCategory = await _transactionRecordCategoryRepository.GetTransactionsCategoryByExternalId(Guid.Parse(categoryExternalId), ctoken);

        if (existingCategory is null)
            return TransactionRecordCategoryErrors.NotFound;

        User? currentUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);

        if (currentUser!.RoleId == (long)UserRoleEnum.Admin)
            return await _transactionRecordCategoryRepository.DeleteTransactionCategory(existingCategory, ctoken);

        if (existingCategory.UserId != currentUser.Id)
            return TransactionRecordCategoryErrors.NotOwner;

        return await _transactionRecordCategoryRepository.DeleteTransactionCategory(existingCategory, ctoken);
    }

    public async Task<ErrorOr<IEnumerable<GetTransactionRecordCategoryResponseDto>>> GetAllUserTransactionCategories(CancellationToken ctoken = default)
    {
        Guid currentUserExternalId = _currentUserService.UserExternalId;
        User? currentUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);

        IEnumerable<TransactionRecordCategory> userCategories = await _transactionRecordCategoryRepository.GetAllUserTransactionCategories(currentUser.Id, ctoken);

        return userCategories.Select(uc => new GetTransactionRecordCategoryResponseDto
        {
            CategoryName = uc.CategoryName,
            CategoryExternalId = uc.ExternalId
        }).ToList();
    }

    public async Task<ErrorOr<int>> UpdateAllUserTransactionCategories(List<UpdateTransactionRecordCategoryRequestDto> request, CancellationToken ctoken = default)
    {
        await _updateCategoriesValidator.ValidateAndThrowAsync(request, ctoken);

        Guid curentUserExternalId = _currentUserService.UserExternalId;
        User? currentUser = await _userRepository.GetUserByExternalId(curentUserExternalId, ctoken);

        var requestParsedData = request
            .Select(c => new
            {
                categoryExternalId = Guid.Parse(c.CategoryExternalId),
                categoryName = c.CategoryName
            })
            .ToList();

        List<Guid> externalIds = requestParsedData
            .Select(rp => rp.categoryExternalId)
            .ToList();

        IEnumerable<TransactionRecordCategory> existingCategories = await _transactionRecordCategoryRepository.GetUserCategoriesByExternalIds(currentUser!.Id, externalIds, ctoken);

        var categoryLookup = existingCategories.ToDictionary(c => c.ExternalId);

        var missingCategories = requestParsedData
            .Where(r => !categoryLookup.ContainsKey(r.categoryExternalId))
            .ToList();

        if (missingCategories.Any())
            return TransactionRecordCategoryErrors.InvalidArgs;

        try
        {
            List<TransactionRecordCategory> mappedCategories = requestParsedData.Select(c =>
            {
                TransactionRecordCategory category = categoryLookup[c.categoryExternalId];
                {
                    category.CategoryName = c.categoryName;
                };

                return category;
            }).ToList();

            return await _transactionRecordCategoryRepository.SaveChanges(ctoken);

        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            return TransactionRecordCategoryErrors.DuplicatedEntry;
        }
    }

    public async Task<ErrorOr<int>> UpdateUserTransactionCategory(UpdateTransactionRecordCategoryRequestDto request, CancellationToken ctoken = default)
    {
        await _updateCategoryValidator.ValidateAndThrowAsync(request, ctoken);

        Guid currentUserExternalId = _currentUserService.UserExternalId;
        User? currentUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);

        TransactionRecordCategory? existingCategory = await _transactionRecordCategoryRepository.GetTransactionsCategoryByExternalId(Guid.Parse(request.CategoryExternalId), ctoken);

        if (existingCategory is null)
            return TransactionRecordCategoryErrors.InvalidArgs;

        if (existingCategory!.UserId != currentUser!.Id)
            return TransactionRecordCategoryErrors.NotOwner;

        try
        {
            existingCategory.CategoryName = request.CategoryName;

            return await _transactionRecordCategoryRepository.SaveChanges(ctoken);

        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            return TransactionRecordCategoryErrors.DuplicatedEntry;
        }
    }
}
