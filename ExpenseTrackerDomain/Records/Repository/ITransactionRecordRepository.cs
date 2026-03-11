using ExpenseTracker.Domain.Records.Entity;

namespace ExpenseTracker.Domain.Records.Repository;

public interface ITransactionRecordRepository
{
    Task<TransactionRecord> AddTransaction(TransactionRecord record, CancellationToken ctoken = default);

    Task<int> DeleteTransactionRecord(TransactionRecord record, CancellationToken ctoken = default);

    Task<TransactionRecord?> GetTransactionRecordByExternalId(Guid externalId, CancellationToken ctoken = default);

    Task<IEnumerable<TransactionRecord>> GetAllUserTransactionsByCategory(long userId, long categoryId, CancellationToken ctoken = default);

    Task<IEnumerable<TransactionRecord>> GetAllUserTransactionsByCollection(long userId, long collectionId, CancellationToken ctoken = default);

    Task<int> UpdateTransaction(TransactionRecord record, CancellationToken ctoken = default);

    Task<int> UpdateAllUserTransactions(List<TransactionRecord> records, CancellationToken ctoken = default);

    Task<IEnumerable<TransactionRecord>> GetUserTransactionsByExternalId(long userId, List<Guid> recordExternalId, CancellationToken ctoken = default);

    Task<TransactionRecord?> GetUserTransactionByCategoryExternalId(Guid recordExternalId, Guid categoryExternalId, CancellationToken ctoken = default);
}
