using Bogus;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Categories.Entity;

namespace ExpenseTracker.IntegrationTests.Categories.Builder;

public class TransactionRecordCategoryBuilder
{
    private readonly Faker _faker = new Faker();

    private string _categoryName;
    private long _userId;
    private User _user;
    private Guid _externalId;

    public TransactionRecordCategoryBuilder()
    {
        _categoryName = _faker.Commerce.Categories(1)[0];
        _externalId = _faker.Random.Guid();
    }

    public TransactionRecordCategoryBuilder WithCategoryName(string categoryName)
    {
        _categoryName = categoryName;
        return this;
    }

    public TransactionRecordCategoryBuilder WithUserId(long userId)
    {
        _userId = userId;
        return this;
    }

    public TransactionRecordCategoryBuilder WithUser(User user)
    {
        _user = user;
        _userId = user?.Id ?? 0;
        return this;
    }

    public TransactionRecordCategory Build()
    {
        return new TransactionRecordCategory
        {
            ExternalId = _externalId,
            CategoryName = _categoryName,
            UserId = _userId,
            User = _user
        };
    }
}