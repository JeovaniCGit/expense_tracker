using ExpenseTracker.Application.Emails.Model;

namespace ExpenseTracker.Application.Emails.Services;
public interface IEmailService
{
    Task<EmailSenderResult> SendVerificationEmail(string to, string receiverName, string verificationToken, long userId, CancellationToken ctoken = default);
    Task<EmailSenderResult> SendPasswordResetEmail(string to, string receiverName, string verificationToken, long userId, CancellationToken ctoken = default);
}
