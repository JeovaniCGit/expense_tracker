using ErrorOr;
using ExpenseTracker.Application.Categories.Contracts.Requests;
using ExpenseTracker.Application.Categories.Contracts.Responses;

namespace ExpenseTracker.Application.Categories.Services;

public interface ITransactionRecordCategoryService
{
    Task<ErrorOr<AddTransactionRecordCategoryResponseDto>> AddUserTransactionRecordCategory(AddTransactionRecordCategoryRequestDto request, CancellationToken ctoken = default);
    Task<ErrorOr<int>> DeleteTransactionRecordCategory(string userExternalId, string categoryExternalId, CancellationToken ctoken = default);
    Task<ErrorOr<int>> UpdateUserTransactionCategory(string userExternalId, UpdateTransactionRecordCategoryRequestDto request, CancellationToken ctoken = default);
    Task<ErrorOr<int>> UpdateAllUserTransactionCategories(string userExternalId, List<UpdateTransactionRecordCategoryRequestDto> request, CancellationToken ctoken = default);
    Task<ErrorOr<IEnumerable<GetTransactionRecordCategoryResponseDto>>> GetAllUserTransactionCategories(string userExternalId, CancellationToken ctoken = default);
}
