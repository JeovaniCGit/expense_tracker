using ExpenseTracker.Domain.Authorization.Tokens.Entity;

namespace ExpenseTracker.Domain.Authorization.Tokens.Repository;

public interface ITokenRepository
{
    Task<Token> AddToken(Token token, CancellationToken ctoken = default);
    Task<Token?> GetTokenByTokenValue(string tokenValue, CancellationToken ctoken = default);
    Task<bool> DeleteExpiredTokens();
    Task<bool> ApplyBehaviorChanges(CancellationToken ctoken = default);
}
