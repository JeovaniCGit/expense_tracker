using ExpenseTracker.Domain.Base.Entity;

namespace ExpenseTracker.Domain.Accounts.Entity;

public class PasswordHistory : BaseEntity
{
    public long UserId { get; set; }
    public string PasswordHash { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public User User { get; set; }

    public PasswordHistory() { }
}
