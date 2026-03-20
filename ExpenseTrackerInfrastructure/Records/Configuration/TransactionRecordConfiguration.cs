using ExpenseTracker.Domain.Records.Entity;
using ExpenseTracker.Infrastructure.Base.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Records.Configuration;

internal sealed class TransactionRecordConfiguration : BaseEntityConfiguration<TransactionRecord>
{
    public override void Configure(EntityTypeBuilder<TransactionRecord> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.ExternalId)
            .IsRequired();

        builder.HasIndex(u => u.ExternalId)
           .IsUnique();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.TransactionValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(p => new { p.TransactionValue, p.TransactionUserId, p.TransactionCategoryId })
            .IsUnique();

        builder.Property(p => p.TransactionUserId)
            .IsRequired();

        builder.Property(p => p.TransactionCategoryId)
            .IsRequired();

        builder.HasOne(t => t.User)
            .WithMany(u => u.Transactions)
            .HasForeignKey(fk => fk.TransactionUserId);

        builder.HasOne(t => t.TransactionCategory)
            .WithMany()
            .HasForeignKey(fk => fk.TransactionCategoryId);

        builder.HasQueryFilter(tr => !tr.User.IsDeleted && !tr.IsDeleted);
    }
}
