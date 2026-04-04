namespace ExpenseTracker.Application.Emails.Exceptions;

public sealed class EmailDeliveryFailedException : Exception
{
    public EmailDeliveryFailedException() : base() { }
    public EmailDeliveryFailedException(string message) : base(message) { }
    public EmailDeliveryFailedException(string message, Exception innerException) : base(message, innerException) { }
}
