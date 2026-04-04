namespace ExpenseTracker.Application.Accounts.Contracts.Responses;
public sealed record UpdateUserResponseDto
{
    public required string Firstname { get; init; }

    public required string Lastname { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}
