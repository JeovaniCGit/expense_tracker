namespace ExpenseTracker.Application.Authorization.Perms.Attributes;

public static class PermissionNames
{
    public const string Admin = "Admin";

    public const string UserRead = "User.Read";
    public const string UserWrite = "User.Write";
    public const string UserDelete = "User.Delete";

    public const string RecordRead = "Record.Read";
    public const string RecordWrite = "Record.Write";
    public const string RecordDelete = "Record.Delete";

    public const string CategoryRead = "Category.Read";
    public const string CategoryWrite = "Category.Write";
    public const string CategoryDelete = "Category.Delete";

    public const string CollectionRead = "Collection.Read";
    public const string CollectionWrite = "Collection.Write";
    public const string CollectionDelete = "Collection.Delete";
}
