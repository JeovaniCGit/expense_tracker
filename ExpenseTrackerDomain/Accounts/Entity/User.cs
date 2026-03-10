using ExpenseTracker.Domain.Authorization.UserRoles.Entity;
using ExpenseTracker.Domain.Base.Entity;
using ExpenseTracker.Domain.Collections.Entity;
using ExpenseTracker.Domain.Records.Entity;

namespace ExpenseTracker.Domain.Accounts.Entity;

public class User : AuditEntity
{
    public string Firstname { get; set; }

    public string Lastname { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public DateTimeOffset PasswordLastUpdated { get; set; } = DateTimeOffset.Now;

    public bool IsEmailVerified { get; set; } = false;

    public long RoleId { get; set; }

    public UserRole Role { get; set; }
    public PasswordHistory PasswordHistory { get; set; }
    public ICollection<TransactionRecord> Transactions { get; set; } = new List<TransactionRecord>();
    public ICollection<TransactionCollection> Collections { get; set; } = new List<TransactionCollection>();

    public User() { }
}
