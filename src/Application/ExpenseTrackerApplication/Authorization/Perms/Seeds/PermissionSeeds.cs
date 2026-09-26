namespace ExpenseTracker.Application.Authorization.Perms.Seeds;

public sealed record PermissionSeed(string PermissionName, string Description);
public static class PermissionSeeds
{
   public static readonly PermissionSeed UserRead =
        new("User.Read", "Read user information");

    public static readonly PermissionSeed UserWrite =
        new("User.Write", "Write user information");

    public static readonly PermissionSeed UserDelete =
        new("User.Delete", "Delete user information");

    public static readonly PermissionSeed RecordRead =
        new("Record.Read", "Read record information");

    public static readonly PermissionSeed RecordWrite =
        new("Record.Write", "Write record information");

    public static readonly PermissionSeed RecordDelete =
        new("Record.Delete", "Delete record information");

    public static readonly PermissionSeed CategoryRead =
        new("Category.Read", "Read category information");

    public static readonly PermissionSeed CategoryWrite =
        new("Category.Write", "Write category information");

    public static readonly PermissionSeed CategoryDelete =
        new("Category.Delete", "Delete category information");

    public static readonly PermissionSeed CollectionRead =
        new("Collection.Read", "Read collection information");

    public static readonly PermissionSeed CollectionWrite =
        new("Collection.Write", "Write collection information");

    public static readonly PermissionSeed    CollectionDelete =
        new("Collection.Delete", "Delete collection information");

    public static IEnumerable<PermissionSeed> All =>
        new[]
        {
            UserRead, UserWrite, UserDelete,
            RecordRead, RecordWrite, RecordDelete,
            CategoryRead, CategoryWrite, CategoryDelete,
            CollectionRead, CollectionWrite, CollectionDelete
        };
}
