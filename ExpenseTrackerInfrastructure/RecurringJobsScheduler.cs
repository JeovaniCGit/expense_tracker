using ExpenseTracker.Infrastructure.Authorization.Tokens.Jobs;
using Hangfire;

namespace ExpenseTracker.Infrastructure;

public class RecurringJobsScheduler
{
    private readonly IRecurringJobManager _jobManager;

    public RecurringJobsScheduler(IRecurringJobManager jobManager)
    {
        _jobManager = jobManager;
    }

    public void AddRecurringJobs()
    {
        _jobManager.AddOrUpdate<DeleteExpiredTokensJob>(
            DeleteExpiredTokensJob.Name,
            job => job.DeleteExpiredTokens(),
            Cron.Hourly());
    }
}
