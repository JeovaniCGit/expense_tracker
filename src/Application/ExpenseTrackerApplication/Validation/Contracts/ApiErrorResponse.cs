namespace ExpenseTracker.Application.Validation.Contracts;

public sealed class ApiErrorResponse
{
    public required List<ApiError> Errors { get; init; }
}

public sealed class ApiError
{
    public required int Code { get; init; }
    public required string? Message { get; init; }
}
