using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Extensions;

public static class ErrorMapping
{
    public static ActionResult MapToStatusCode(this List<Error> errors)
    {
        int statusCode = errors.First().Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        return new ObjectResult(errors)
        {
            StatusCode = statusCode,
        };
    }
}
