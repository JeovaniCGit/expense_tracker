using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Authorization.Tokens.Entity;
using ExpenseTracker.Domain.Base.Entity;
using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Domain.Records.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ExpenseTracker.Infrastructure.Database.Interceptors.Audit;

internal sealed class UpdateAuditableInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _context;
    private readonly IDateProvider _dateProvider;
    public UpdateAuditableInterceptor(IHttpContextAccessor context, IDateProvider dateProvider)
    {
        _context = context;
        _dateProvider = dateProvider;
    }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context, _dateProvider);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAuditableEntities(DbContext context, IDateProvider dateProvider, IHttpContextAccessor? _context = default)
    {
        DateTimeOffset utcNow = dateProvider.Now;
        List<EntityEntry<AuditEntity>> entities = context.ChangeTracker.Entries<AuditEntity>().ToList();
        string? executerUser = _context!.HttpContext?.User?.Identity?.Name;
        executerUser = executerUser ?? "System";

        foreach (EntityEntry<AuditEntity> entry in entities)
        {

            switch (entry.State)
            {
                case EntityState.Added:
                    SetCurrentPropertyDatetimeValue(entry, nameof(AuditEntity.CreatedAt), utcNow);
                    entry.Property("CreatedBy").CurrentValue = executerUser;
                    break;

                case EntityState.Modified:
                    if (entry.Entity is User && entry.Property("Password").IsModified && !Equals(entry.Property("Password").OriginalValue, entry.Property("Password").CurrentValue))
                        SetCurrentPropertyDatetimeValue(entry, nameof(User.PasswordLastUpdated), utcNow);

                    SetCurrentPropertyDatetimeValue(entry, nameof(AuditEntity.UpdatedAt), utcNow);
                    entry.Property("UpdatedBy").CurrentValue = executerUser;
                    break;

                case EntityState.Deleted:
                    if (entry.Entity is Token || entry.Entity is TransactionRecord || entry.Entity is TransactionRecordCategory)
                    {
                        break;
                    }
                    entry.State = EntityState.Modified;
                    entry.Property(nameof(AuditEntity.DeletedAt)).CurrentValue = utcNow;
                    entry.Property("DeletedBy").CurrentValue = executerUser;
                    entry.CurrentValues["IsDeleted"] = true;
                    break;
            }
        }
    }

    static void SetCurrentPropertyDatetimeValue(EntityEntry entry, string propertyName, DateTimeOffset utcNow)
    {
        entry.Property(propertyName).CurrentValue = utcNow;
    }
}
