using ErrorOr;

namespace ExpenseTracker.Application.Categories.Errors;
public sealed class TransactionRecordCategoryErrors
{
    public static Error NotFound =>
        Error.NotFound("TransactionCategory.NotFound", "Category not found.");

    public static Error NotOwner =>
        Error.Unauthorized("TransactionCategory.NotOwner", "Unauthorized.");

    public static Error InvalidArgs =>
        Error.Validation("TransactionCategory.InvalidArgs", "Error, invalid arguments.");

    public static Error DuplicatedEntry =>
        Error.Conflict("TransactionCategory.DuplicatedEntry", "Error, category already exists.");

    public static Error ConcurrencyConflict =>
        Error.Conflict("TransactionCategory.ConcurrencyConflict", "The resource was modified by another process. Please reload and try again.");
}
