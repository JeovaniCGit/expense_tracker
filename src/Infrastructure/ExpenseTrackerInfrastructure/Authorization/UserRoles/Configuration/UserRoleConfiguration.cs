using ExpenseTracker.Application.Abstractions.GuidSeeder;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Domain.Authorization.UserRoles.Entity;
using ExpenseTracker.Infrastructure.Base.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Authorization.UserRoles.Configuration;

internal sealed class UserRoleConfiguration : BaseEntityConfiguration<UserRole>
{
    public override void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(p => p.ExternalId)
            .IsRequired();

        builder.HasIndex(u => u.ExternalId)
           .IsUnique();

        builder.Property(p => p.UserRoleName)
            .HasMaxLength(200)
            .IsRequired();

        var roleIds = Enum.GetValues(typeof(UserRoleEnum))
            .Cast<UserRoleEnum>()
            .Select(r => (long)r);

        var roleSeed = roleIds.Select(id => new UserRole
        {
            Id = id,
            ExternalId = GuidSeed.CreateGuidFromName(((UserRoleEnum)id).ToString()),
            UserRoleName = ((UserRoleEnum)id).ToString()
        }).ToArray();

        builder.HasData(roleSeed);
    }
}
