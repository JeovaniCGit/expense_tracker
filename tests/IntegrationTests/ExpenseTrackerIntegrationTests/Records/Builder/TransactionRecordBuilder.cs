using Bogus;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Domain.Collections.Entity;
using ExpenseTracker.Domain.Records.Entity;

namespace ExpenseTracker.IntegrationTests.Records.Builder;

public class TransactionRecordBuilder
{
    private readonly Faker _faker = new Faker();

    private decimal _transactionValue;
    private long _transactionUserId;
    private long _transactionCategoryId;
    private long _transactionCollectionId;
    private TransactionRecordCategory _transactionCategory;
    private TransactionCollection _transactionCollection;
    private User _user;
    private Guid _externalId;

    public TransactionRecordBuilder()
    {
        _transactionValue = _faker.Finance.Amount(1, 1000);
        _transactionUserId = 0;
        _transactionCategoryId = 0;
        _transactionCollectionId = 0;
        _externalId = _faker.Random.Guid();
    }

    public TransactionRecordBuilder WithTransactionValue(decimal transactionValue)
    {
        _transactionValue = transactionValue;
        return this;
    }

    public TransactionRecordBuilder WithTransactionUserId(long transactionUserId)
    {
        _transactionUserId = transactionUserId;
        return this;
    }

    public TransactionRecordBuilder WithTransactionCategoryId(long transactionCategoryId)
    {
        _transactionCategoryId = transactionCategoryId;
        return this;
    }

    public TransactionRecordBuilder WithTransactionCollectionId(long transactionCollectionId)
    {
        _transactionCollectionId = transactionCollectionId;
        return this;
    }

    public TransactionRecordBuilder WithTransactionCategory(TransactionRecordCategory transactionCategory)
    {
        _transactionCategory = transactionCategory;
        _transactionCategoryId = transactionCategory?.Id ?? 0;
        return this;
    }

    public TransactionRecordBuilder WithTransactionCollection(TransactionCollection transactionCollection)
    {
        _transactionCollection = transactionCollection;
        _transactionCollectionId = transactionCollection?.Id ?? 0;
        return this;
    }

    public TransactionRecordBuilder WithUser(User user)
    {
        _user = user;
        _transactionUserId = user?.Id ?? 0;
        return this;
    }

    public TransactionRecordBuilder WithExternalId(Guid externalId)
    {
        _externalId = externalId;
        return this;
    }

    public TransactionRecord Build()
    {
        return new TransactionRecord
        {
            ExternalId = _externalId,
            TransactionValue = _transactionValue,
            TransactionUserId = _transactionUserId,
            TransactionCategoryId = _transactionCategoryId,
            TransactionCollectionId = _transactionCollectionId,
            TransactionCategory = _transactionCategory,
            TransactionCollection = _transactionCollection,
            User = _user
        };
    }
}