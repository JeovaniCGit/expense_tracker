using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Infrastructure.Base.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Accounts.Configuration;

internal sealed class UserConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.ExternalId)
            .IsRequired();

        builder.HasIndex(u => u.ExternalId)
           .IsUnique();

        builder.Property(p => p.Firstname)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Lastname)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Email)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(p => p.Password)
            .IsRequired();

        builder.Property(p => p.RoleId)
            .IsRequired();

        builder.HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(fk => fk.RoleId);

        builder.Property(p => p.RoleId)
            .IsRequired();

        builder.Property(p => p.RoleId)
            .IsRequired();

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.Property<uint>("xmin")
            .IsRowVersion();
    }
}
