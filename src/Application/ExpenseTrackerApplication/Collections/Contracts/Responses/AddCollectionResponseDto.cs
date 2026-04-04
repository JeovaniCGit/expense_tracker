namespace ExpenseTracker.Application.Collections.Contracts.Responses;
public sealed record AddCollectionResponseDto
{
    public required Guid ExternalId { get; set; }
    public required string Description { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
