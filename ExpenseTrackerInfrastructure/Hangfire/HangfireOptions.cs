namespace ExpenseTracker.Infrastructure.Hangfire;

public sealed class HangfireOptions
{
    public string ServerName { get; set; }
    public int SchedulePollingIntervalInSeconds { get; set; }
}
