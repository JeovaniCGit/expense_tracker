using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Base.Entity;

namespace ExpenseTracker.Domain.Authorization.Tokens.Entity;

public class Token : AuditEntity
{
    public string TokenValue { get; set; }
    public long TokenTypeId { get; set; }
    public long TokenUserId { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTimeOffset? UsedAt { get; set; }
    public TokenType TokenType { get; set; }
    public User User { get; set; }

    public Token() { }
}
