using ExpenseTracker.Domain.Base.Entity;

namespace ExpenseTracker.Domain.Authorization.Tokens.Entity;

public class TokenType : BaseEntity
{
    public string TokenTypeDescription { get; set; }
    public int TimeToLiveInMinutes { get; set; }
    public ICollection<Token> Tokens { get; set; } = new List<Token>();

    public TokenType() { }
}
