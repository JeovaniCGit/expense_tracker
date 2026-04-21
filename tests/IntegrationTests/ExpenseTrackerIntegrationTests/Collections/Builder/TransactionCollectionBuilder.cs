using Bogus;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Collections.Entity;
using ExpenseTracker.Domain.Records.Entity;

namespace ExpenseTracker.IntegrationTests.Collections.Builder;

public class TransactionCollectionBuilder
{
    private readonly Faker _faker = new Faker();

    private string _description;
    private long _userId;
    private decimal _estimatedBudget;
    private decimal _realBudget;
    private DateTimeOffset _startDate;
    private DateTimeOffset _endDate;
    private User _user;
    private ICollection<TransactionRecord> _records;
    private Guid _externalId;

    public TransactionCollectionBuilder()
    {
        _description = _faker.Lorem.Sentence();
        _estimatedBudget = _faker.Finance.Amount(100, 10000);
        _realBudget = _faker.Finance.Amount(50, 5000);
        _startDate = _faker.Date.PastOffset().ToUniversalTime();
        _endDate = _faker.Date.FutureOffset().ToUniversalTime();
        _records = new List<TransactionRecord>();
        _externalId = _faker.Random.Guid();
    }

    public TransactionCollectionBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public TransactionCollectionBuilder WithUserId(long userId)
    {
        _userId = userId;
        return this;
    }

    public TransactionCollectionBuilder WithEstimatedBudget(decimal estimatedBudget)
    {
        _estimatedBudget = estimatedBudget;
        return this;
    }

    public TransactionCollectionBuilder WithRealBudget(decimal realBudget)
    {
        _realBudget = realBudget;
        return this;
    }

    public TransactionCollectionBuilder WithStartDate(DateTimeOffset startDate)
    {
        _startDate = startDate;
        return this;
    }

    public TransactionCollectionBuilder WithEndDate(DateTimeOffset endDate)
    {
        _endDate = endDate;
        return this;
    }

    public TransactionCollectionBuilder WithUser(User user)
    {
        _user = user;
        _userId = user?.Id ?? 0;
        return this;
    }

    public TransactionCollectionBuilder WithRecords(ICollection<TransactionRecord> records)
    {
        _records = records;
        return this;
    }

    public TransactionCollectionBuilder WithExternalId(Guid externalId)
    {
        _externalId = externalId;
        return this;
    }

    public TransactionCollection Build()
    {
        return new TransactionCollection
        {
            ExternalId = _externalId,
            Description = _description,
            UserId = _userId,
            EstimatedBudget = _estimatedBudget,
            RealBudget = _realBudget,
            StartDate = _startDate,
            EndDate = _endDate,
            User = _user,
            Records = _records
        };
    }
}