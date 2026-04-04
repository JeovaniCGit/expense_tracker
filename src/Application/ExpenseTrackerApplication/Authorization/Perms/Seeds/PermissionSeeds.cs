namespace ExpenseTracker.Application.Authorization.Perms.Seeds;

public sealed record PermissionSeed(long Id, string PermissionName, string Description);
public static class PermissionSeeds
{
    public static readonly PermissionSeed Admin =
        new(1, "Admin", "Admin privileges");

    public static readonly PermissionSeed UserRead =
        new(2, "User.Read", "Read user information");

    public static readonly PermissionSeed UserWrite =
        new(3, "User.Write", "Write user information");

    public static readonly PermissionSeed UserDelete =
        new(4, "User.Delete", "Delete user information");

    public static readonly PermissionSeed RecordRead =
        new(5, "Record.Read", "Read record information");

    public static readonly PermissionSeed RecordWrite =
        new(6, "Record.Write", "Write record information");

    public static readonly PermissionSeed RecordDelete =
        new(7, "Record.Delete", "Delete record information");

    public static readonly PermissionSeed CategoryRead =
        new(8, "Category.Read", "Read category information");

    public static readonly PermissionSeed CategoryWrite =
        new(9, "Category.Write", "Write category information");

    public static readonly PermissionSeed CategoryDelete =
        new(10, "Category.Delete", "Delete category information");

    public static readonly PermissionSeed CollectionRead =
        new(11, "Collection.Read", "Read collection information");

    public static readonly PermissionSeed CollectionWrite =
        new(12, "Collection.Write", "Write collection information");

    public static readonly PermissionSeed    CollectionDelete =
        new(13, "Collection.Delete", "Delete collection information");

    public static IEnumerable<PermissionSeed> All =>
        new[]
        {
            Admin, UserRead, UserWrite, UserDelete,
            RecordRead, RecordWrite, RecordDelete,
            CategoryRead, CategoryWrite, CategoryDelete,
            CollectionRead, CollectionWrite, CollectionDelete
        };
}
