using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Domain.Categories.Repository;
using ExpenseTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Categories.Repository;

public class TransactionRecordCategoryRepository : ITransactionRecordCategoryRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRecordCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionRecordCategory> AddTransactionCategory(TransactionRecordCategory transactionCategory, CancellationToken ctoken = default)
    {
        await _context.TransactionRecordCategories.AddAsync(transactionCategory);
        await _context.SaveChangesAsync(ctoken);
        return transactionCategory;
    }

    public async Task<int> DeleteTransactionCategory(TransactionRecordCategory transactionCategory, CancellationToken ctoken = default)
    {
        _context.TransactionRecordCategories.Remove(transactionCategory);
        int affected = await _context.SaveChangesAsync(ctoken);
        return affected;
    }

    public async Task<long?> GetTransactionCategoryIdByExternalId(Guid externalId, CancellationToken ctoken = default)
    {
        return await _context.TransactionRecordCategories.Where(tc => tc.ExternalId == externalId).Select(tc => (long?)tc.Id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TransactionRecordCategory>> GetAllTransactionsCategories(CancellationToken ctoken = default)
    {
        return await _context.TransactionRecordCategories.AsNoTracking()
            .OrderBy(tc => tc.CategoryName)
            .Select(tc => new TransactionRecordCategory
            {
                ExternalId = tc.ExternalId,
                CategoryName = tc.CategoryName,
                UserId = tc.UserId
            })
            .ToListAsync(ctoken);
    }

    public async Task<TransactionRecordCategory?> GetTransactionsCategoryByExternalId(Guid externalId, CancellationToken ctoken = default)
    {
        return await _context.TransactionRecordCategories.AsNoTracking().FirstOrDefaultAsync(tc => tc.ExternalId == externalId, ctoken);
    }

    public async Task<int> SaveChanges(CancellationToken ctoken = default)
    {
        int affected = await _context.SaveChangesAsync(ctoken);
        return affected;
    }

    public async Task<IEnumerable<TransactionRecordCategory>> GetAllUserTransactionCategories(long userId, CancellationToken ctoken = default)
    {
        return await _context.TransactionRecordCategories.AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderBy(tc => tc.CategoryName)
            .Select(tc => new TransactionRecordCategory
            {
                ExternalId = tc.ExternalId,
                CategoryName = tc.CategoryName,
                UserId = tc.UserId
            })
            .ToListAsync(ctoken);
    }

    public async Task<IEnumerable<TransactionRecordCategory>> GetUserCategoriesByExternalIds(long userId, List<Guid> categoryExternalIds, CancellationToken ctoken = default)
    {
        return await _context.TransactionRecordCategories
            .Where(tc => tc.UserId == userId && categoryExternalIds
            .Contains(tc.ExternalId))
            .ToListAsync(ctoken);
    }

    public async Task<TransactionRecordCategory?> GetUserCategoryByCategoryName(long userId, string categoryName, CancellationToken ctoken = default)
    {
        return await _context.TransactionRecordCategories
            .Where(tc => tc.UserId == userId && tc.CategoryName == categoryName)
            .FirstOrDefaultAsync(ctoken);
    }
}
