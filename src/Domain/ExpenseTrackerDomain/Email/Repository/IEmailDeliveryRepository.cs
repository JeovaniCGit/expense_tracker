using ExpenseTracker.Domain.Email.Entity;

namespace ExpenseTracker.Domain.Email.Repository;

public interface IEmailDeliveryRepository
{
    Task<int> SaveChanges(EmailDelivery email);
    Task<string> GetEmailStatusByUserId(long userId);
    int Add(EmailDelivery email);
}
