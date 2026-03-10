using ErrorOr;
using ExpenseTracker.Application.Records.Contracts.Requests;
using ExpenseTracker.Application.Records.Contracts.Responses;

namespace ExpenseTracker.Application.Records.Services;

public interface ITransactionRecordService
{
    Task<ErrorOr<AddTransactionRecordResponseDto>> AddUserTransactionRecord(AddTransactionRecordRequestDto request, CancellationToken ctoken = default);
    Task<ErrorOr<int>> DeleteTransactionRecord(string userExternalId, string recordExternalId, CancellationToken ctoken = default);
    Task<ErrorOr<IEnumerable<GetTransactionRecordResponseDto>>> GetAllUserTransactionsByCategory(string userExternalId, string categoryExternalId, CancellationToken ctoken = default);
    Task<ErrorOr<int>> UpdateUserTransaction(string userExternalId, UpdateTransactionRecordRequestDto request, CancellationToken ctoken = default);
    Task<ErrorOr<int>> UpdateAllUserTransactions(string userExternalId, List<UpdateTransactionRecordRequestDto> request, CancellationToken ctoken = default);
    Task<ErrorOr<IEnumerable<GetTransactionRecordResponseDto>>> GetAllTransactionsByCollectionId(string collectionExternalId, CancellationToken ctoken = default);
}
