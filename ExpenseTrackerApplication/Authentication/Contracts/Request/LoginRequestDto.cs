namespace ExpenseTracker.Application.Authentication.Contracts.Request;
public sealed record LoginRequestDto
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
