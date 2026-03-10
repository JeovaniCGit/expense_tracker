namespace ExpenseTracker.Application.Authentication.JwtLib;
public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Guid userId, List<string> userPermissions, string email, CancellationToken ctoken = default);
    string GenerateRefreshToken(Guid userId, string email, CancellationToken ctoken = default);
}
