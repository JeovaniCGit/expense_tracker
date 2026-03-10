namespace ExpenseTracker.Application.Records.Contracts.Requests;
public sealed record UpdateTransactionRecordRequestDto
{
    public required string TransactionExternalId {  get; init; }
    public required string TransactionCategoryExternalId {  get; init; }
    public required decimal TransactionValue { get; init; } = 0.0m;
}
