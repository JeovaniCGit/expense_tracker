namespace ExpenseTracker.Application.Authentication.Contracts.Response;
public sealed record LoginResponseDto
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}
