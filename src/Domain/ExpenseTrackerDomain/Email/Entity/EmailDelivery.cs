using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Base.Entity;

namespace ExpenseTracker.Domain.Email.Entity;

public class EmailDelivery : BaseEntity
{
    public long UserId { get; set; }
    public string Status { get; set; }
    public DateTimeOffset? SentAt { get; set; } = null;
    public User User { get; set; }

    public EmailDelivery() { }
}
