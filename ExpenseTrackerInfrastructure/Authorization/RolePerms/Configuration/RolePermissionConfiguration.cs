using ExpenseTracker.Application.Abstractions.GuidSeeder;
using ExpenseTracker.Application.Authorization.Perms.Seeds;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Domain.Authorization.RolePerms.Entity;
using ExpenseTracker.Infrastructure.Base.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Authorization.RolePerms.Configuration;

internal sealed class RolePermissionConfiguration : BaseEntityConfiguration<RolePermission>
{
    public override void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(k => new { k.RoleId, k.PermissionId });

        builder.Property(p => p.RoleId)
            .IsRequired();

        builder.Property(p => p.PermissionId)
            .IsRequired();

        builder.HasOne(r => r.Role)
            .WithMany(rp => rp.RolePermissions)
            .HasForeignKey(rp => rp.RoleId);

        builder.HasOne(p => p.Permission)
            .WithMany(rp => rp.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId);

        List<PermissionSeed> permissions = PermissionSeeds.All.ToList();

        long id = 1;

        var systemPermissionsSeed = permissions
            .Where(perm => perm.PermissionName != PermissionSeeds.Admin.PermissionName)
            .Select(perm => new RolePermission
            {
                RoleId = (long)UserRoleEnum.System,
                PermissionId = perm.Id,
                ExternalId = GuidSeed.CreateGuidFromName($"{UserRoleEnum.System}_{perm.PermissionName}")
            });

        var adminPermissionsSeed = permissions
            .Select(perm => new RolePermission
            {
                RoleId = (long)UserRoleEnum.Admin,
                PermissionId = perm.Id,
                ExternalId = GuidSeed.CreateGuidFromName($"{UserRoleEnum.Admin}_{perm.PermissionName}")
            });

        var regularUserPermissionsSeed = permissions
            .Where(perm => perm.PermissionName != PermissionSeeds.Admin.PermissionName)
            .Select(perm => new RolePermission
            {
                RoleId = (long)UserRoleEnum.RegularUser,
                PermissionId = perm.Id,
                ExternalId = GuidSeed.CreateGuidFromName($"{UserRoleEnum.RegularUser}_{perm.PermissionName}")
            });

        builder.HasData(systemPermissionsSeed
                .Concat(adminPermissionsSeed)
                .Concat(regularUserPermissionsSeed)
                .ToArray());
    }
}
