using ExpenseTracker.Domain.Accounts.Entity;

namespace ExpenseTracker.Domain.Accounts.Repository;

public interface IPasswordHistoryRepository
{
        Task<PasswordHistory?> GetByPasswordHash(string passwordHash, CancellationToken ctoken = default);
        Task<IEnumerable<PasswordHistory>> GetHistoryOfPasswordHashes(long userId, CancellationToken ctoken = default);
        Task<int> Add(PasswordHistory passwordHistory, CancellationToken ctoken = default);
}