using ExpenseTracker.Domain.Authorization.Perms.Entity;
using ExpenseTracker.Domain.Authorization.UserRoles.Entity;
using ExpenseTracker.Domain.Base.Entity;

namespace ExpenseTracker.Domain.Authorization.RolePerms.Entity;

public class RolePermission
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
    public Guid ExternalId { get; init; }
    public UserRole Role { get; set; }
    public Permission Permission { get; set; }

    public RolePermission() { }
}
