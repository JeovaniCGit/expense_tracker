namespace ExpenseTracker.Application.Categories.Contracts.Responses;
public sealed record GetTransactionRecordCategoryResponseDto
{
    public required string CategoryName { get; init; }
    public required Guid CategoryExternalId { get; init; }
}
