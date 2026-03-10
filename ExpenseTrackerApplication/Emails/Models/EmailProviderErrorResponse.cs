namespace ExpenseTracker.Application.Emails.Models;

public sealed class EmailProviderErrorResponse
{
    public List<EmailProviderError> Errors { get; init; } = new List<EmailProviderError>();
}

public sealed class EmailProviderError
{
    public string? Field { get; init; }
    public string? Message { get; init; }
}
