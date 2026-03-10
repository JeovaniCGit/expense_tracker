namespace ExpenseTracker.Application.Accounts.Contracts.Requests;
public sealed record UpdateUserRequestDto
{
    public required string UserExternalId { get; init; }

    public string? Firstname { get; init; }

    public string? Lastname { get; init; }

    public string? Email { get; init; }

    public string? Password { get; init; }

}
