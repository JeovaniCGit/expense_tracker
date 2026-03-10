namespace ExpenseTracker.Application.Records.Contracts.Responses;
public sealed record AddTransactionRecordResponseDto
{
    public required decimal TransactionValue { get; init; }
    public required Guid ExternalId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
