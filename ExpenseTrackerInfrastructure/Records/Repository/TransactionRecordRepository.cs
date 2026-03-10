using ExpenseTracker.Domain.Records.Entity;
using ExpenseTracker.Domain.Records.Repository;
using ExpenseTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Records.Repository;

public class TransactionRecordRepository : ITransactionRecordRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRecordRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionRecord> AddTransaction(TransactionRecord record, CancellationToken ctoken = default)
    {
        await _context.TransactionRecords.AddAsync(record);
        await _context.SaveChangesAsync(ctoken);
        return record;
    }

    public async Task<int> DeleteTransactionRecord(TransactionRecord record, CancellationToken ctoken = default)
    {
        _context.TransactionRecords.Remove(record);
        int affected = await _context.SaveChangesAsync(ctoken);
        return affected;
    }

    public async Task<int> UpdateTransactionRecord(TransactionRecord record, CancellationToken ctoken = default)
    {
        _context.TransactionRecords.Update(record);
        int affected = await _context.SaveChangesAsync(ctoken);
        return affected;
    }

    public async Task<TransactionRecord?> GetTransactionRecordById(long id, CancellationToken ctoken = default)
    {
        return await _context.TransactionRecords.AsNoTracking().FirstOrDefaultAsync(tr => tr.Id == id, ctoken);
    }

    public async Task<IEnumerable<TransactionRecord>> GetAllTransactionRecords(CancellationToken ctoken = default)
    {
        return await _context.TransactionRecords.AsNoTracking()
            .Include(tr => tr.TransactionCategory)
            .OrderBy(tr => tr.TransactionCategory.CategoryName)
            .Select(tr => new TransactionRecord
            {
                ExternalId = tr.ExternalId,
                TransactionValue = tr.TransactionValue,
                TransactionCategory = tr.TransactionCategory
            })
            .ToListAsync(ctoken);
    }

    public async Task<long?> GetTransactionRecordIdByExternalId(Guid externalId, CancellationToken ctoken = default)
    {
        TransactionRecord? record = await _context.TransactionRecords.AsNoTracking().FirstOrDefaultAsync(tr => tr.ExternalId == externalId, ctoken);
        if (record is null)
            return null;

        return record.Id;
    }

    public async Task<TransactionRecord?> GetTransactionRecordByExternalId(Guid externalId, CancellationToken ctoken = default)
    {
        TransactionRecord? record = await _context.TransactionRecords.AsNoTracking().FirstOrDefaultAsync(tr => tr.ExternalId == externalId, ctoken);
        return record;
    }

    #region User related actions

    public async Task<IEnumerable<TransactionRecord>> GetAllUserTransactionsByCategory(long userId, long categoryId, CancellationToken ctoken = default)
    {
        return await _context.TransactionRecords.AsNoTracking()
            .Where(t => t.TransactionCategoryId == categoryId && t.TransactionUserId == userId)
            .Include(tr => tr.TransactionCategory)
            .OrderBy(tr => tr.TransactionCategory.CategoryName)
            .Select(tr => new TransactionRecord
            {
                ExternalId = tr.ExternalId,
                TransactionValue = tr.TransactionValue,
                TransactionCategory = tr.TransactionCategory
            })
            .ToListAsync(ctoken);
    }

    public async Task<TransactionRecord?> GetUserTransactionByCategory(long userId, long categoryId, CancellationToken ctoken = default)
    {
        return await _context.TransactionRecords.AsNoTracking().FirstOrDefaultAsync(t => t.TransactionCategoryId == categoryId && t.TransactionUserId == userId, ctoken);
    }

    public async Task<int> UpdateUserTransaction(TransactionRecord record, CancellationToken ctoken = default)
    {
        _context.TransactionRecords.Update(record);
        int affected = await _context.SaveChangesAsync(ctoken);
        return affected;
    }

    public async Task<int> UpdateAllUserTransactions(List<TransactionRecord> records, CancellationToken ctoken = default)
    {
        _context.TransactionRecords.UpdateRange(records);
        int affected = await _context.SaveChangesAsync(ctoken);
        return affected;
    }

    public async Task<IEnumerable<TransactionRecord>> GetAllUserTransactionsByCollection(long userId, long collectionId, CancellationToken ctoken = default)
    {
        return await _context.TransactionRecords.AsNoTracking()
            .Where(tr => tr.TransactionUserId == userId && tr.TransactionCollectionId == collectionId)
            .Include(tr => tr.TransactionCategory)
            .OrderBy(tr => tr.TransactionCategory.CategoryName)
            .Select(tr => new TransactionRecord
            {
                ExternalId = tr.ExternalId,
                TransactionValue = tr.TransactionValue,
                TransactionCategory = tr.TransactionCategory
            })
            .ToListAsync(ctoken);
    }
    #endregion
}
