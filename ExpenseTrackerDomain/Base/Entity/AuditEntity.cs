namespace ExpenseTracker.Domain.Base.Entity;

public abstract class AuditEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public bool IsDeleted { get; init; } = false;
    public string? CreatedBy { get; init; }
    public string? UpdatedBy { get; set; }
    public string? DeletedBy { get; set; }
}
