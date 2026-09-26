using ErrorOr;
using ExpenseTracker.Application.Abstractions.DbExceptionHandler;
using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Application.Categories.Contracts.Requests;
using ExpenseTracker.Application.Categories.Contracts.Responses;
using ExpenseTracker.Application.Categories.Errors;
using ExpenseTracker.Application.Records.Errors;
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

        Guid existingUserExternalId = _currentUserService.UserExternalId;
        var existingUser = await _userRepository.GetUserByExternalId(existingUserExternalId, ctoken);
        try
        {
            TransactionRecordCategory addedCategory = await _transactionRecordCategoryRepository.AddTransactionCategory(
                new TransactionRecordCategory { CategoryName = request.CategoryName, UserId = existingUser!.Id }, ctoken);

            return new AddTransactionRecordCategoryResponseDto
            {
                CategoryExternalId = addedCategory.ExternalId,
                CategoryName = addedCategory.CategoryName,
                CreatedAt = addedCategory.CreatedAt,
                Version = addedCategory.Version
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
        User? existingUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);
        
        var list = new List<Guid> { Guid.Parse(categoryExternalId) };
        var existingCategories = await _transactionRecordCategoryRepository.GetUserCategoriesByExternalIds(existingUser!.Id, list, ctoken);
        if (!existingCategories.Any())
            return TransactionRecordCategoryErrors.InvalidArgs;
        
        return await _transactionRecordCategoryRepository.DeleteTransactionCategory(existingCategories.First(), ctoken);
    }

    public async Task<ErrorOr<IEnumerable<GetTransactionRecordCategoryResponseDto>>> GetAllUserTransactionCategories(CancellationToken ctoken = default)
    {
        Guid currentUserExternalId = _currentUserService.UserExternalId;
        User? currentUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);

        IEnumerable<TransactionRecordCategory> userCategories = await _transactionRecordCategoryRepository.GetAllUserTransactionCategories(currentUser!.Id, ctoken);

        return userCategories.Select(uc => new GetTransactionRecordCategoryResponseDto
        {
            CategoryName = uc.CategoryName,
            CategoryExternalId = uc.ExternalId,
            Version = uc.Version
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
        catch (DbUpdateConcurrencyException ex)
        {
            return TransactionRecordCategoryErrors.ConcurrencyConflict;
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
        
        var categoriesList = new List<Guid> { Guid.Parse(request.CategoryExternalId) };
        var categories = await _transactionRecordCategoryRepository.GetUserCategoriesByExternalIds(currentUser!.Id, categoriesList, ctoken);

        if (!categories.Any())
            return TransactionRecordCategoryErrors.InvalidArgs;

        try
        {
            var tc = categories!.First();
            tc.CategoryName = request.CategoryName;

            return await _transactionRecordCategoryRepository.SaveChanges(ctoken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return TransactionRecordCategoryErrors.ConcurrencyConflict;
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            return TransactionRecordCategoryErrors.DuplicatedEntry;
        }
    }
}
