namespace ExpenseTracker.Application.Authentication.Contracts.Request;

public sealed record RefreshRequestDto
{
    public required string RefreshToken { get; init; }
}
