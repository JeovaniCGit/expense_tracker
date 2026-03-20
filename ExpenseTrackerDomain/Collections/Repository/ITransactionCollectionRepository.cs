using ExpenseTracker.Domain.Collections.Entity;

namespace ExpenseTracker.Domain.Collection.Repository;
public interface ITransactionCollectionRepository
{
    Task<TransactionCollection> AddCollection(TransactionCollection transactionCollection, CancellationToken ctoken = default);
    Task<int> SaveChanges(CancellationToken ctoken = default);
    Task<int> DeleteCollection(TransactionCollection transactionCollection, CancellationToken ctoken = default);
    Task<long?> GetCollectionIdByExternalId(Guid externalId, CancellationToken ctoken = default);
    Task<TransactionCollection?> GetCollectionByExternalId(Guid externalId, CancellationToken ctoken = default);
    Task<TransactionCollection?> GetUserCollectionByExternalId(long userId, Guid collectionExternalId, CancellationToken ctoken = default);
    Task<IEnumerable<TransactionCollection>> GetAllUserCollections(long userId, DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken ctoken = default);
}
