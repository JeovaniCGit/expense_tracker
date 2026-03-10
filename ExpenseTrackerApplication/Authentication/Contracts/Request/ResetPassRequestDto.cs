namespace ExpenseTracker.Application.Authentication.Contracts.Request;

public sealed record ResetPassRequestDto
{
    public required string Password { get; init; }
}
