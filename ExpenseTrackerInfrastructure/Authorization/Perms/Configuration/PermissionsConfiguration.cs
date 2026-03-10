using ExpenseTracker.Application.Abstractions.GuidSeeder;
using ExpenseTracker.Application.Authorization.Perms.Seeds;
using ExpenseTracker.Domain.Authorization.Perms.Entity;
using ExpenseTracker.Infrastructure.Base.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Authorization.Perms.Configuration;

internal sealed class PermissionsConfiguration : BaseEntityConfiguration<Permission>
{
    public override void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(p => p.ExternalId)
            .IsRequired();

        builder.HasIndex(p => p.ExternalId)
           .IsUnique();

        builder.Property(p => p.PermissionName)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.PermissionDescription)
            .HasMaxLength(500)
            .IsRequired();

        List<PermissionSeed> permissions = PermissionSeeds.All.ToList();

        builder.HasData(permissions.Select(p =>
            new Permission
            {
                Id = p.Id,
                ExternalId = GuidSeed.CreateGuidFromName(p.PermissionName),
                PermissionName = p.PermissionName,
                PermissionDescription = p.Description
            }).ToArray()
        );
    }
}
