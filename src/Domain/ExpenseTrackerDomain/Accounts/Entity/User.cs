using ExpenseTracker.Domain.Base.Entity;
using ExpenseTracker.Domain.Collections.Entity;
using ExpenseTracker.Domain.Records.Entity;

namespace ExpenseTracker.Domain.Accounts.Entity;

public class User : AuditEntity
{
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
    public uint Version { get; set; }
    public ICollection<TransactionRecord> Transactions { get; set; } = new List<TransactionRecord>();
    public ICollection<TransactionCollection> Collections { get; set; } = new List<TransactionCollection>();

    public User() { }
}
