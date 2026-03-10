using ExpenseTracker.Domain.Authorization.Tokens.Repository;
using ExpenseTracker.Domain.Authorization.Tokens.Entity;
using System.Security.Cryptography;
using System.Text;
using ExpenseTracker.Application.Authorization.Tokens.Enums;

namespace ExpenseTracker.Application.Authorization.Tokens.Services;
public sealed class TokenService : ITokenService
{
    private readonly ITokenRepository _tokenRepository;

    public TokenService(ITokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    public async Task<Token> AddToken(Token token, CancellationToken ctoken = default)
    {
        return await _tokenRepository.AddToken(token, ctoken);
    }

    public Token GenerateToken(TokenDescriptionEnum tokenDescription, long userId, CancellationToken ctoken = default)
    {
        string generatedToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return HashToken(generatedToken, tokenDescription, userId, ctoken);
    }

    public Token HashToken(string token, TokenDescriptionEnum tokenDescription, long userId, CancellationToken ctoken = default)
    {
        string hashedToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        return new Token
        {
            TokenValue = hashedToken,
            TokenTypeId = (long)tokenDescription,
            TokenUserId = userId
        };
    }

    public async Task<Token?> GetTokenByTokenValue (string value, CancellationToken ctoken = default)
    {
        return await _tokenRepository.GetTokenByTokenValue(value, ctoken);
    }

    public async Task<bool> ApplyBehaviorChanges(CancellationToken ctoken = default)
    {
        return await _tokenRepository.ApplyBehaviorChanges(ctoken);
    }
}
