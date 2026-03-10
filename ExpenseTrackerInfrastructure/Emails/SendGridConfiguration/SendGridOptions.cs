namespace ExpenseTracker.Infrastructure.Emails.SendGridConfiguration;

public sealed class SendGridOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string VerificationTemplateId { get; set; } = string.Empty;
    public string ResetTemplateId { get; set; } = string.Empty;
}
