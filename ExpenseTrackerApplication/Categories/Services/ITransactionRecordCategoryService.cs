using ErrorOr;
using ExpenseTracker.Application.Categories.Contracts.Requests;
using ExpenseTracker.Application.Categories.Contracts.Responses;

namespace ExpenseTracker.Application.Categories.Services;

public interface ITransactionRecordCategoryService
{
    Task<ErrorOr<AddTransactionRecordCategoryResponseDto>> AddUserTransactionRecordCategory(AddTransactionRecordCategoryRequestDto request, CancellationToken ctoken = default);
    Task<ErrorOr<int>> DeleteTransactionRecordCategory(string categoryExternalId, CancellationToken ctoken = default);
    Task<ErrorOr<int>> UpdateUserTransactionCategory(UpdateTransactionRecordCategoryRequestDto request, CancellationToken ctoken = default);
    Task<ErrorOr<int>> UpdateAllUserTransactionCategories(List<UpdateTransactionRecordCategoryRequestDto> request, CancellationToken ctoken = default);
    Task<ErrorOr<IEnumerable<GetTransactionRecordCategoryResponseDto>>> GetAllUserTransactionCategories(CancellationToken ctoken = default);
}
