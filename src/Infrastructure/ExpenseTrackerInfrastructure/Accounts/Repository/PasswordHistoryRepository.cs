using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Accounts.Repository;

public sealed class PasswordHistoryRepository : IPasswordHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<int> Add(PasswordHistory passwordHistory, CancellationToken ctoken = default)
    {
        await _context.PasswordHistory.AddAsync(passwordHistory);
        int affected = await _context.SaveChangesAsync();
        return affected;
    }

    public async Task<PasswordHistory?> GetByPasswordHash(string passwordHash, CancellationToken ctoken = default)
    {
        return await _context.PasswordHistory
            .AsNoTracking()
            .FirstOrDefaultAsync(ph => ph.PasswordHash == passwordHash, ctoken);
    }
}
