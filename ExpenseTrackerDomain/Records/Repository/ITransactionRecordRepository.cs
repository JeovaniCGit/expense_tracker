using ExpenseTracker.Domain.Records.Entity;

namespace ExpenseTracker.Domain.Records.Repository;

public interface ITransactionRecordRepository
{
    Task<TransactionRecord> AddTransaction(TransactionRecord record, CancellationToken ctoken = default);
    Task<int> DeleteTransactionRecord(TransactionRecord record, CancellationToken ctoken = default);
    Task<int> UpdateTransactionRecord(TransactionRecord record, CancellationToken ctoken = default);
    Task<TransactionRecord?> GetTransactionRecordById(long id, CancellationToken ctoken = default);
    Task<long?> GetTransactionRecordIdByExternalId(Guid externalId, CancellationToken ctoken = default);
    Task<TransactionRecord?> GetTransactionRecordByExternalId(Guid externalId, CancellationToken ctoken = default);
    Task<IEnumerable<TransactionRecord>> GetAllTransactionRecords(CancellationToken ctoken = default);

    #region User related actions
    Task<IEnumerable<TransactionRecord>> GetAllUserTransactionsByCategory(long userId, long categoryId, CancellationToken ctoken = default);
    Task<IEnumerable<TransactionRecord>> GetAllUserTransactionsByCollection(long userId, long collectionId, CancellationToken ctoken = default);
    Task<TransactionRecord?> GetUserTransactionByCategory(long userId, long categoryId, CancellationToken ctoken = default);
    Task<int> UpdateUserTransaction(TransactionRecord record, CancellationToken ctoken = default);
    Task<int> UpdateAllUserTransactions(List<TransactionRecord> records, CancellationToken ctoken = default);
    #endregion
}
