using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Accounts.Repository;

internal sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User> CreateUser(User user, CancellationToken ctoken = default)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync(ctoken);
        return user;
    }

    public async Task<int> DeleteUser(User user, CancellationToken ctoken = default)
    {
        var entity = await _context.Users.FindAsync(user.Id);
        if (entity == null)
            return 0;

        _context.Users.Remove(entity);
        int affected = await _context.SaveChangesAsync(ctoken);
        return affected;
    }

    public async Task<User?> GetUserById(long userId, CancellationToken ctoken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Transactions)
            .ThenInclude(t => t.TransactionCategory)
            .FirstOrDefaultAsync(u => u.Id == userId, ctoken);
    }

    public async Task<User?> GetUserByEmail(string email, CancellationToken ctoken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email.Equals(email), ctoken);
    }

    public async Task<User?> GetUserByExternalId(Guid id, CancellationToken ctoken = default)
    {
        return await _context.Users
            .Include(u => u.PasswordHistory)
            .FirstOrDefaultAsync(u => u.ExternalId == id, ctoken);
    }

    public async Task<int> UpdateUser(User user, CancellationToken ctoken = default)
    {
        if (string.IsNullOrEmpty(user.Password))
            _context.Entry(user).Property(u => u.Password).IsModified = false;

        int affected = await _context.SaveChangesAsync(ctoken);
        return affected;
    }

    public async Task<bool> ApplyBehaviorChanges(CancellationToken ctoken = default)
    {
        await _context.SaveChangesAsync(ctoken);
        return true;
    }

    public async Task<IEnumerable<User>> GetAllUsers(int page, int pageSize, CancellationToken ctoken = default)
    {
        return await _context.Users.AsNoTracking()
            .OrderBy(u => u.Firstname)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new User
            {
                Firstname = u.Firstname,
                Lastname = u.Lastname,
                Email = u.Email,
                ExternalId = u.ExternalId,
            }).ToListAsync(ctoken);
    }
}

