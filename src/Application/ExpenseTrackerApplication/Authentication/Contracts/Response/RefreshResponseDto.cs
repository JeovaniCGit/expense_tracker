namespace ExpenseTracker.Application.Authentication.Contracts.Response;

public sealed record RefreshResponseDto
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}
