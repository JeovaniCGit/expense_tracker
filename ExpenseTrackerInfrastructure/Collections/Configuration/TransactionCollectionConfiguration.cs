using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Collections.Entity;
using ExpenseTracker.Infrastructure.Base.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Collections.Configuration;
internal sealed class TransactionCollectionConfiguration : BaseEntityConfiguration<TransactionCollection>
{
    public override void Configure(EntityTypeBuilder<TransactionCollection> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(p => p.Id)
           .ValueGeneratedOnAdd();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.ExternalId)
            .IsRequired();

        builder.HasIndex(p => p.ExternalId)
           .IsUnique();

        builder.Property(p => p.Description)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.EstimatedBudget)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.RealBudget)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.StartDate)
            .IsRequired();

        builder.HasIndex(p => new { p.UserId, p.StartDate });

        builder.HasIndex(p => new { p.UserId, p.StartDate, p.EndDate, p.Description })
            .IsUnique();

        builder.Property(p => p.EndDate)
            .IsRequired();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.HasOne(tc => tc.User)
            .WithMany(u => u.Collections)
            .HasForeignKey(tc => tc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(tc => !tc.User.IsDeleted && !tc.IsDeleted);
    }
}
