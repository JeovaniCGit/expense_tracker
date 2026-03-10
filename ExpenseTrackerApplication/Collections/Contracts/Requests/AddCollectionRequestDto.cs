namespace ExpenseTracker.Application.Collections.Contracts.Requests;
public sealed record AddCollectionRequestDto
{
    public required string Description { get; init; }
    public required string UserExternalId { get; init; }
    public required decimal EstimatedBudget { get; init; } = 0.0m;
    public required decimal RealBudget { get; init; } = 0.0m;
    public required DateTimeOffset StartDate { get; init; }
    public required DateTimeOffset EndDate { get; init; }
}
