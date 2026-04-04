namespace ExpenseTracker.Application.Categories.Contracts.Requests;
public sealed record UpdateTransactionRecordCategoryRequestDto
{
    public required string CategoryExternalId { get; init; }
    public required string CategoryName { get; init; }
}
