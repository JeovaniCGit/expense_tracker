using ErrorOr;

namespace ExpenseTracker.Application.Collections.Errors;
public static class CollectionErrors
{
    public static Error NotFound =>
        Error.NotFound("Collection.NotFound", "Collection not found.");

    public static Error InvalidArgs =>
        Error.Validation("Collection.InvalidArgs", "Error, invalid arguments.");

    public static Error Unauthorized =>
        Error.Unauthorized("Collection.Unauthorized", "Error, unauthorized action.");

    public static Error NotOwner =>
        Error.Unauthorized("Collection.NotOwner", "Unauthorized.");

    public static Error DuplicatedEntry =>
        Error.Conflict("Collection.DuplicatedEntry", "Error, collection already exists.");

    public static Error ConcurrencyConflict =>
        Error.Conflict("Collection.ConcurrencyConflict", "The resource was modified by another process. Please reload and try again.");
}
