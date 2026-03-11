using ExpenseTracker.Domain.Categories.Entity;
namespace ExpenseTracker.Domain.Categories.Repository;

public interface ITransactionRecordCategoryRepository
{
    Task<TransactionRecordCategory> AddTransactionCategory(TransactionRecordCategory transactionCategory, CancellationToken ctoken = default);
    Task<IEnumerable<TransactionRecordCategory>> GetAllTransactionsCategories(CancellationToken ctoken = default);
    Task<TransactionRecordCategory?> GetTransactionRecordCategoryById(long transactionCategoryId, CancellationToken ctoken = default);
    Task<long?> GetTransactionCategoryIdByExternalId(Guid externalId, CancellationToken ctoken = default);
    Task<TransactionRecordCategory?> GetTransactionsCategoryByExternalId(Guid externalId, CancellationToken ctoken = default);
    Task<int> DeleteTransactionCategory(TransactionRecordCategory transactionCategory, CancellationToken ctoken = default);
    Task<TransactionRecordCategory> UpdateTransactionRecordCategory(TransactionRecordCategory transactionCategory, CancellationToken ctoken = default);

    #region User related actions
    Task<int> UpdateUserTransactionCategory(TransactionRecordCategory category, CancellationToken ctoken = default);
    Task<int> UpdateAllUserTransactionCategories(List<TransactionRecordCategory> categories, CancellationToken ctoken = default);
    Task<IEnumerable<TransactionRecordCategory>> GetAllUserTransactionCategories(long userId, CancellationToken ctoken = default);

    Task<IEnumerable<TransactionRecordCategory>> GetUserCategoriesByExternalIds(long userId, List<Guid> categoryExternalId, CancellationToken ctoken = default);
    #endregion
}
