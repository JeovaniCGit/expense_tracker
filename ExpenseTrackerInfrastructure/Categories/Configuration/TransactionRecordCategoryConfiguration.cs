using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Infrastructure.Base.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Categories.Configuration;

internal sealed class TransactionRecordCategoryConfiguration : BaseEntityConfiguration<TransactionRecordCategory>
{
    public override void Configure(EntityTypeBuilder<TransactionRecordCategory> builder)
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

        builder.Property(p => p.CategoryName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.HasOne(trc => trc.User)
            .WithMany()
            .HasForeignKey(trc => trc.UserId);

        builder.HasQueryFilter(trc => !trc.User.IsDeleted && !trc.IsDeleted);
    }
}
