namespace ExpenseTracker.Application.Collections.Contracts.Responses;
public sealed record UpdateCollectionResponseDto
{
    public required string Description { get; init; }
    public required decimal EstimatedBudget { get; init; }
    public required decimal RealBudget { get; init; }
    public required DateTimeOffset StartDate { get; init; }
    public required DateTimeOffset EndDate { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
