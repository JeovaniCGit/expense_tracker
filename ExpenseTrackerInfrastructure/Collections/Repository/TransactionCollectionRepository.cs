using ExpenseTracker.Domain.Collection.Repository;
using ExpenseTracker.Domain.Collections.Entity;
using ExpenseTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Collections.Repository;
internal class TransactionCollectionRepository : ITransactionCollectionRepository
{
    private readonly ApplicationDbContext _context;
    public TransactionCollectionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionCollection> AddCollection(TransactionCollection transactionCollection, CancellationToken ctoken = default)
    {
        await _context.Collections.AddAsync(transactionCollection, ctoken);
        await _context.SaveChangesAsync(ctoken);
        return transactionCollection;
    }

    public async Task<int> DeleteCollection(TransactionCollection transactionCollection, CancellationToken ctoken = default)
    {
        _context.Collections.Remove(transactionCollection);
        int affected = await _context.SaveChangesAsync(ctoken);
        return affected;
    }

    public async Task<long?> GetCollectionIdByExternalId(Guid externalId, CancellationToken ctoken = default)
    {
        return await _context.Collections.AsNoTracking().Where(c => c.ExternalId == externalId).Select(c => c.Id).FirstOrDefaultAsync();
    }

    public async Task<TransactionCollection?> GetCollectionByExternalId(Guid externalId, CancellationToken ctoken = default)
    {
        return await _context.Collections.AsNoTracking().Where(c => c.ExternalId == externalId).FirstOrDefaultAsync();
    }

    public async Task<int> UpdateCollection(TransactionCollection transactionCollection, CancellationToken ctoken = default)
    {
        _context.Collections.Update(transactionCollection);
        int affected = await _context.SaveChangesAsync(ctoken);
        return affected;
    }

    public async Task<IEnumerable<TransactionCollection>> GetAllUserCollections(long userId, DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken ctoken = default)
    {
        IQueryable<TransactionCollection> query = _context.Collections
            .AsNoTracking()
            .Where(c => c.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(c => c.StartDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(c => c.EndDate < endDate.Value);

        return await query
        .OrderBy(c => c.StartDate)
        .ToListAsync(ctoken);
    }
}
 