namespace ExpenseTracker.Application.Abstractions.DateTimeProvider;

public interface IDateProvider
{
    DateTimeOffset Now { get; }
}
