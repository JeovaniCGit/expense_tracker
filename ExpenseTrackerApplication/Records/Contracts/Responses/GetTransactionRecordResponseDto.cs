namespace ExpenseTracker.Application.Records.Contracts.Responses;
public sealed record GetTransactionRecordResponseDto
{
    public required decimal TransactionValue { get; init; }

    public required Guid TransactionExternalId { get; init; }

    public required Guid TransactionCategoryExternalId { get; init; }

    public required string TransactionCategoryName { get; init; }

}
