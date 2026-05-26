using ExpenseTracker.Application.Validation.Contracts;
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
            var validationFailureResponse = new ApiErrorResponse
            {
                Errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .Select(item => new ApiError
                        {
                            Code = context.Response.StatusCode,
                            Message = item.First().ErrorMessage
                        }
                    ).ToList()
            };

            await context.Response.WriteAsJsonAsync(validationFailureResponse);
        }
    }
}
