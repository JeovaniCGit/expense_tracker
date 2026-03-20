using ExpenseTracker.Application.Records.Contracts.Responses;

namespace ExpenseTracker.Application.Collections.Contracts.Responses;
public sealed record GetCollectionResponseDto
{
    public required string Description { get; init; }
    public required Guid CollectionExternalId { get; init; }
    public required decimal EstimatedBudget { get; init; }
    public required decimal RealBudget { get; init; }
    public required DateTimeOffset StartDate { get; init; }
    public required DateTimeOffset EndDate { get; init; }
}
