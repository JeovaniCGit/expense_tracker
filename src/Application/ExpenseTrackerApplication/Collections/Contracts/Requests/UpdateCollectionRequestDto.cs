namespace ExpenseTracker.Application.Collections.Contracts.Requests;
public sealed record UpdateCollectionRequestDto
{
    public string? Description { get; init; }
    public decimal? EstimatedBudget { get; init; }
    public decimal? RealBudget { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public required string CollectionExternalId { get; init; }
}
