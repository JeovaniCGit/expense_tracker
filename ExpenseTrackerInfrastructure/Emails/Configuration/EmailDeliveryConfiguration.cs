using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Email.Entity;
using ExpenseTracker.Infrastructure.Base.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Emails.Configuration;

public sealed class EmailDeliveryConfiguration : BaseEntityConfiguration<EmailDelivery>
{
    public override void Configure(EntityTypeBuilder<EmailDelivery> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.ExternalId)
            .IsRequired();

        builder.HasIndex(u => u.ExternalId)
           .IsUnique();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.HasIndex(p => p.UserId)
            .IsUnique();

        builder.Property(p => p.Status)
            .IsRequired();

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<EmailDelivery>(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
