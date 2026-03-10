namespace ExpenseTracker.Application.Accounts.Contracts.Responses;

public sealed record GetAllUsersResponseDto
{
    public required Guid UserExternalId { get; set; }
    public required string Firstname { get; init; }
    public required string Lastname { get; init; }
    public required string Email { get; init; }
}
