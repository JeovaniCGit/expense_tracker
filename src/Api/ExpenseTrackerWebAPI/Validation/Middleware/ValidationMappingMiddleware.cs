using ExpenseTracker.Application.Accounts.Contracts.Responses;
using FluentValidation;

namespace ExpenseTracker.API.Validation.Middleware;

public sealed class ValidationMappingMiddleware
{
    private readonly RequestDelegate _next;
    public ValidationMappingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            var validationFailureResponse = new ValidationFailureResponse
            {
                Errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToList()
                    )
            };

            await context.Response.WriteAsJsonAsync(validationFailureResponse);
        }
    }
}
