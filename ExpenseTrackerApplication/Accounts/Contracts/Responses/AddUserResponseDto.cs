namespace ExpenseTracker.Application.Accounts.Contracts.Responses;
public sealed record AddUserResponseDto
{
    public required Guid ExternalId { get; init; }
    public required string Firstname { get; init; }

    public required string Lastname { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}
