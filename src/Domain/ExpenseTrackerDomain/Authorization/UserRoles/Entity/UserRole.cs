using ExpenseTracker.Domain.Authorization.RolePerms.Entity;
using ExpenseTracker.Domain.Base.Entity;

namespace ExpenseTracker.Domain.Authorization.UserRoles.Entity;

public class UserRole : BaseEntity
{
    public string UserRoleName { get; set; }
    public ICollection<RolePermission> RolePermissions = new List<RolePermission>();

    public UserRole() { }
}
