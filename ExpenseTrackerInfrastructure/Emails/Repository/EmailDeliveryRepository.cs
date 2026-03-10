using ExpenseTracker.Domain.Email.Entity;
using ExpenseTracker.Domain.Email.Repository;
using ExpenseTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Emails.Repository;

public sealed class EmailDeliveryRepository : IEmailDeliveryRepository
{
    private readonly ApplicationDbContext _context;

    public EmailDeliveryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetEmailStatusByUserId(long userId)
    {
        return await _context.EmailDeliveries
            .Where(e => e.UserId == userId)
            .Select(e => e.Status)
            .FirstAsync();
    }

    public async Task<int> Update(EmailDelivery email)
    {
        _context.EmailDeliveries.Update(email);
        int affected = await _context.SaveChangesAsync();
        return affected;
    }

    public int Add(EmailDelivery email)
    {
        _context.EmailDeliveries.AddAsync(email);
        int affected = _context.SaveChanges();
        return affected;
    }
}
