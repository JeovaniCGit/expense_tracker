using ErrorOr;
using ExpenseTracker.Application.Records.Contracts.Requests;
using ExpenseTracker.Application.Records.Contracts.Responses;

namespace ExpenseTracker.Application.Records.Services;

public interface ITransactionRecordService
{
    Task<ErrorOr<AddTransactionRecordResponseDto>> AddUserTransactionRecord(AddTransactionRecordRequestDto request, CancellationToken ctoken = default);
    Task<ErrorOr<int>> DeleteTransactionRecord(string recordExternalId, CancellationToken ctoken = default);
    Task<ErrorOr<IEnumerable<GetTransactionRecordResponseDto>>> GetAllUserTransactionsByCategory(string categoryExternalId, CancellationToken ctoken = default);
    Task<ErrorOr<int>> UpdateUserTransaction(UpdateTransactionRecordRequestDto request, CancellationToken ctoken = default);
    Task<ErrorOr<int>> UpdateAllUserTransactions(List<UpdateTransactionRecordRequestDto> request, CancellationToken ctoken = default);
    Task<ErrorOr<IEnumerable<GetTransactionRecordResponseDto>>> GetAllTransactionsByCollectionId(string collectionExternalId, CancellationToken ctoken = default);
}
