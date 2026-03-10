namespace ExpenseTracker.Application.Categories.Contracts.Responses;
public sealed record UpdateTransactionRecordCategoryResponseDto
{
    public required string CategoryName { get; init; }
    public required Guid CategoryExternalId { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
