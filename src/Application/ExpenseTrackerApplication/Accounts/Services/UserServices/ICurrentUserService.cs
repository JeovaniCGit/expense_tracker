namespace ExpenseTracker.Application.Accounts.Services.UserServices;

public interface ICurrentUserService
{
    Guid UserExternalId { get; }
}
