using ExpenseTracker.Application.Accounts.Contracts.Responses;

namespace ExpenseTracker.Application.Accounts.Services.AdminServices;

public interface IAdminAnalyticsService
{
    Task<GetUserAnalyticsResponseDto> GetUsersAnalytics(CancellationToken cancellationToken = default);
}
