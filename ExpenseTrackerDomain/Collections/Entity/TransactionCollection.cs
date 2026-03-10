using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Base.Entity;
using ExpenseTracker.Domain.Records.Entity;

namespace ExpenseTracker.Domain.Collections.Entity;
public class TransactionCollection : AuditEntity
{
    public string Description { get; set; }
    public long UserId { get; set; }
    public decimal EstimatedBudget { get; set; } = 0.0m;
    public decimal RealBudget { get; set; } = 0.0m;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public User User { get; set; }
    public ICollection<TransactionRecord> Records { get; set; } = new List<TransactionRecord>();
    public TransactionCollection() { }
}
