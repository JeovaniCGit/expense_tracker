using ExpenseTracker.Application.Abstractions.DateTimeProvider;

namespace ExpenseTracker.Infrastructure.Abstractions;

public sealed class DateProvider : IDateProvider
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}
