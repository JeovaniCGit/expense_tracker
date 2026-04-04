namespace ExpenseTracker.Application.Accounts.Contracts.Responses;
public sealed record GetUserToValidateResponseDto
{
    public required Guid UserExternalId { get; set; }
}
