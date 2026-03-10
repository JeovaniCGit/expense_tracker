using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Application.Accounts.Services.AdminServices;
using ExpenseTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Accounts.AnalyticsService;

internal sealed class AdminAnalyticsService : IAdminAnalyticsService
{
    private readonly ApplicationDbContext _context;
    public AdminAnalyticsService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<GetUserAnalyticsResponseDto> GetUsersAnalytics(CancellationToken ctoken = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new GetUserAnalyticsResponseDto
            {
                ActiveUsersCount = g.Count(u => !u.IsDeleted),
                TotalUsersCount = g.Count(),
                AverageTransactionCountPerUser = (decimal)g.Average(u => u.Transactions.Count())
            })
            .FirstAsync(ctoken);
    }
}