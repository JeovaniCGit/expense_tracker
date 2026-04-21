namespace ExpenseTracker.Application.Records.Contracts.Requests;
public sealed record AddTransactionRecordRequestDto
{
    public required decimal TransactionValue { get; init; } = 0.0m;
    public required string TransactionUserExternalId { get; init; }
    public required string TransactionCollectionExternalId {get; init; }
    public required string TransactionCategoryExternalId { get; init; }
}
