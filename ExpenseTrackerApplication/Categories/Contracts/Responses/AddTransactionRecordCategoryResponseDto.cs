namespace ExpenseTracker.Application.Categories.Contracts.Responses;
public sealed record AddTransactionRecordCategoryResponseDto
{
    public required Guid CategoryExternalId { get; init; }
    public required string CategoryName { get; init; }
    public required Guid TransactionCategoryUserExternalId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
