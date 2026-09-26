namespace ExpenseTracker.Application.Categories.Contracts.Requests;
public sealed record AddTransactionRecordCategoryRequestDto
{
    public required string CategoryName { get; init; }

}
