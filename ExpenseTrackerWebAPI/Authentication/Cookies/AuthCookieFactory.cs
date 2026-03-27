namespace ExpenseTracker.API.Authentication.Cookies;

public class AuthCookieFactory
{
    private readonly IConfiguration _configuration;

    public AuthCookieFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public CookieOptions CreateAccessTokenCookie()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_configuration.GetValue<int>("JWT_ACCESSTOKEN_EXPIRYMINUTES"))
        };
    }

    public CookieOptions CreateRefreshTokenCookie()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/auth/refresh",
            Expires = DateTimeOffset.UtcNow.AddDays(_configuration.GetValue<int>("JWT_REFRESHTOKEN_EXPIRYDAYS"))
        };
    }
}
