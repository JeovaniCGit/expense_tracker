using ExpenseTracker.Domain.Accounts.Entity;
namespace ExpenseTracker.Domain.Accounts.Repository;

public interface IUserRepository
{
    Task<User> CreateUser(User user, CancellationToken ctoken = default);
    Task<User?> GetUserById(long id, CancellationToken ctoken = default);
    Task<User?> GetUserByEmail(string email, CancellationToken ctoken = default);
    Task<User?> GetUserByExternalId(Guid id, CancellationToken ctoken = default);
    Task<int> UpdateUser(User user, CancellationToken ctoken = default);
    Task<int> DeleteUser(User user, CancellationToken ctoken = default);
    Task<bool> ApplyBehaviorChanges(CancellationToken ctoken = default);
    Task<IEnumerable<User>> GetAllUsers(int page, int pageSize, CancellationToken ctoken = default);
}
