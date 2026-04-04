namespace ExpenseTracker.Application.Abstractions.RateLimitingConstants;

public static class RateLimitingPolicy
{
    public const string AuthenticatedUsers = "authenticatedUserRateLimiter";
    public const string AnonymousUser = "anonymousUserRateLimiter";
}
