using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Base.Entity;
using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Domain.Collections.Entity;

namespace ExpenseTracker.Domain.Records.Entity;

public class TransactionRecord : AuditEntity
{
    public decimal TransactionValue { get;  set; } = 0.0m;
    public long TransactionUserId { get;  set; }
    public long TransactionCategoryId { get;  set; }
    public long TransactionCollectionId { get; set; }
    public TransactionRecordCategory TransactionCategory { get;  set; }
    public TransactionCollection TransactionCollection { get;  set; }
    public User User { get;  set; }
    public TransactionRecord() { }
}
