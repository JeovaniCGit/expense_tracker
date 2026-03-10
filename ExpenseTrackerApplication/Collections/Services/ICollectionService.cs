using ErrorOr;
using ExpenseTracker.Application.Collections.Contracts.Requests;
using ExpenseTracker.Application.Collections.Contracts.Responses;

namespace ExpenseTracker.Application.Collections.Services;

public interface ICollectionService
{
    Task<ErrorOr<AddCollectionResponseDto>> AddCollection(AddCollectionRequestDto request, CancellationToken ctoken = default);
    Task<ErrorOr<int>> UpdateCollection(UpdateCollectionRequestDto request, CancellationToken ctoken = default);
    Task<ErrorOr<int>> DeleteCollection(string collectionExternalId, CancellationToken ctoken = default);
    Task<ErrorOr<IEnumerable<GetCollectionResponseDto>>> GetAllUserCollections(string userExternalId, DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken ctoken = default);
}
