using ExpenseTracker.Application.Authorization.Tokens.Enums;
using ExpenseTracker.Domain.Authorization.Tokens.Entity;

namespace ExpenseTracker.Application.Authorization.Tokens.Services;
public interface ITokenService
{
    Task<Token> AddToken(Token token, CancellationToken ctoken = default);
    Token GenerateToken(TokenDescriptionEnum tokenDescription, long userId, CancellationToken ctoken = default);
    Token HashToken(string token, TokenDescriptionEnum tokenDescription, long userId, CancellationToken ctoken = default);
    Task<Token?> GetTokenByTokenValue(string value, CancellationToken ctoken = default);
    Task<bool> ApplyBehaviorChanges(CancellationToken ctoken = default);
}
