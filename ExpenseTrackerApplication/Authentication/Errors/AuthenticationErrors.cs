using ErrorOr;

namespace ExpenseTracker.Application.Authentication.Errors;

public static class AuthenticationErrors
{
    public static Error Unauthorized =>
        Error.Unauthorized("Authentication.InvalidCredentials", "The provided credentials are invalid.");

    public static Error InvalidArgs =>
        Error.Validation("Authentication.InvalidArgs", "Error. Invalid args.");

    public static Error DuplicatedEntry =>
        Error.Conflict("Authentication.DuplicatedEntry", "User already exists.");

    public static Error InvalidPassword =>
        Error.Conflict("User.InvalidPassword", "Password used recently.");
}
