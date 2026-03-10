namespace ExpenseTracker.Application.Emails.Enums;

public enum EmailSendFailureReason
{
    TransientFailure,
    InvalidRecipient,
    ProviderRejected,
    Unknown,
    AuthenticationFailed,
    ServiceUnavailable,
    RateLimited
}
