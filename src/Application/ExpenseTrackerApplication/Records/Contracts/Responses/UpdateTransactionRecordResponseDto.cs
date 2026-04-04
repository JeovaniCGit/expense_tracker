namespace ExpenseTracker.Application.Records.Contracts.Responses;
public sealed record UpdateTransactionRecordResponseDto
{
    public required decimal TransactionValue { get; init; }

    public required Guid TransactionExternalId { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

}
