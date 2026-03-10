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
}
