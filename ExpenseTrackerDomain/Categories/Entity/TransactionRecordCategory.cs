using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Base.Entity;

namespace ExpenseTracker.Domain.Categories.Entity;

public class TransactionRecordCategory : AuditEntity
{
    public string CategoryName { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }

    public TransactionRecordCategory() { }
}
