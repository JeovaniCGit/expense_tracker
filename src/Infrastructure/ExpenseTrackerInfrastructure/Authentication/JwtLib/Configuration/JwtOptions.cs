namespace ExpenseTracker.Infrastructure.Authentication.JwtLib.Configuration;
public sealed class JwtOptions
{
    public string SigningKey { get; set; } = null!;
    public int AccessTokenExpiryMinutes { get; set; }
    public int RefreshTokenExpiryDays { get; set; }
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
}
