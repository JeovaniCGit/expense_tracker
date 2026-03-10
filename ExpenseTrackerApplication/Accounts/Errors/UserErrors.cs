using ErrorOr;

namespace ExpenseTracker.Application.Accounts.Errors;
public static class UserErrors
{
    public static Error NotFound =>
        Error.NotFound("User.NotFound", "User not found.");

    public static Error DuplicatedEntry =>
        Error.Conflict("User.DuplicatedEntry", "User already exists.");

    public static Error InvalidArgs =>
        Error.Validation("User.InvalidArgs", "Error, invalid arguments.");

    public static Error InvalidRole =>
        Error.Validation("User.InvalidRole", "User role not found.");

    public static Error InvalidPassword =>
        Error.Conflict("User.InvalidPassword", "Password used recently.");

    public static Error Unauthorized =>
        Error.Unauthorized("User.Unauthorized", "Error, the user does not have access to this resource.");

    public static Error Forbidden =>
       Error.Forbidden("User.Forbidden", "Error, operation not allowed.");
}
