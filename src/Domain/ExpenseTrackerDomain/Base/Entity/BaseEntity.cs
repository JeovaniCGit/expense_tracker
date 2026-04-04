namespace ExpenseTracker.Domain.Base.Entity;

public abstract class BaseEntity
{
    public long Id { get; init; }

    public Guid ExternalId { get; init; } = Guid.NewGuid();
}
