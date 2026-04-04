using ExpenseTracker.Domain.Accounts.Entity;

namespace ExpenseTracker.Application.Accounts.Contracts.Requests;
public sealed record AddUserRequestDto
{
    public required string Firstname { get; init; }

    public required string Lastname { get; init; }

    public required string Email { get; init; }

    public required string Password { get; init; }

}
