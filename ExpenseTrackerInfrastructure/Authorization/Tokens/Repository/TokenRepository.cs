using ExpenseTracker.Domain.Authorization.Tokens.Entity;
using ExpenseTracker.Domain.Authorization.Tokens.Repository;
using ExpenseTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Authorization.Tokens.Repository;

public class TokenRepository : ITokenRepository
{
    private readonly ApplicationDbContext _context;
    public TokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Token> AddToken(Token token, CancellationToken ctoken = default)
    {
        await _context.Tokens.AddAsync(token);
        await _context.SaveChangesAsync(ctoken);
        return token;
    }

    public async Task<Token?> GetTokenByTokenValue(string tokenValue, CancellationToken ctoken = default)
    {
        return await _context.Tokens.AsNoTracking()
            .Where(t => t.TokenValue == tokenValue)
            .FirstOrDefaultAsync(ctoken);
    }

    public async Task<bool> DeleteExpiredTokens()
    {
        //For now since the tokens will be hard deleted will keep ExecuteDeleteAsync
        //Later if Soft delete is required, use RemoveRange version
        DateTime now = DateTime.UtcNow;
        await _context.Tokens.AsNoTracking().Where(t => t.CreatedAt.AddMinutes(t.TokenType.TimeToLiveInMinutes) < now || t.IsUsed == true).Select(t => t.Id).ExecuteDeleteAsync();
        return true;
    }

    public async Task<bool> ApplyBehaviorChanges(CancellationToken ctoken = default)
    {
        await _context.SaveChangesAsync(ctoken);
        return true;
    }
}
