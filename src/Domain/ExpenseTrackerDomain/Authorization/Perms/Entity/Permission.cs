using ExpenseTracker.Domain.Authorization.RolePerms.Entity;
using ExpenseTracker.Domain.Base.Entity;

namespace ExpenseTracker.Domain.Authorization.Perms.Entity;

public class Permission : BaseEntity
{
    public string PermissionName { get; set; }
    public string PermissionDescription { get; set; }
    private readonly List<RolePermission> _rolePermissions = new List<RolePermission>();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions;

    public Permission() { }
}
