namespace ExpenseTracker.Application.Accounts.Contracts.Requests;
public sealed record UpdateUserRequestDto
{
    public string? Firstname { get; init; }
    public string? Lastname { get; init; }
    public string? Email { get; init; }
    public required uint Version { get; init; }
}
