namespace ExpenseTracker.Application.Accounts.Contracts.Responses;

public class ValidationFailureResponse
{
    public required IEnumerable<ValidationResponse> Errors { get; init; } = new List<ValidationResponse>();
}

public class ValidationResponse
{
    public required string PropertyName { get; init; }
    public required string Message { get; init; }
}
