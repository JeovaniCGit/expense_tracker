using ExpenseTracker.Application.Emails.Enums;

namespace ExpenseTracker.Application.Emails.Model;
public sealed class EmailSenderResult
{
    public bool Success { get; }
    public EmailSendFailureReason? FailureReason { get; }
    public IReadOnlyList<string> Errors { get; } = Array.Empty<string>();
    private EmailSenderResult(bool success, EmailSendFailureReason? failureReason, IReadOnlyList<string>? errors = null)
    {
        Success = success;
        FailureReason = failureReason;
        Errors = errors ?? Array.Empty<string>();
    }
    public static EmailSenderResult Ok() => new EmailSenderResult(true, null, Array.Empty<string>());
    public static EmailSenderResult Failure(EmailSendFailureReason reason, params string[] errors) => new EmailSenderResult(false, reason, errors);
}
