namespace ExpenseTracker.Application.Accounts.Contracts.Responses;

public sealed record GetUserAnalyticsResponseDto
{
    public required int ActiveUsersCount { get; init; }
    public required int TotalUsersCount { get; init; }
    public required decimal AverageTransactionCountPerUser { get; init; }
}
