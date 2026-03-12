using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Infrastructure.Base.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Accounts.Configuration;

internal sealed class PasswordHistoryConfiguration : BaseEntityConfiguration<PasswordHistory>
{
    public override void Configure(EntityTypeBuilder<PasswordHistory> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.HasOne(ph => ph.User)
            .WithOne(u => u.PasswordHistory)
            .HasForeignKey<PasswordHistory>(ph => ph.UserId);

        builder.Property(p => p.PasswordHash)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();
    }
}
