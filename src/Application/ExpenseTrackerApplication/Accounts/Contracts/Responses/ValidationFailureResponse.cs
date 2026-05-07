namespace ExpenseTracker.Application.Accounts.Contracts.Responses;

public class ValidationFailureResponse
{
    public required Dictionary<string, List<string>> Errors { get; init; } = new Dictionary<string, List<string>>();
}
