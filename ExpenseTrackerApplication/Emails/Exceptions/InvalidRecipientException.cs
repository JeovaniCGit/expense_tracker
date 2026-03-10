namespace ExpenseTracker.Infrastructure.Emails.Exceptions;

public sealed class InvalidRecipientException : Exception
{ 
    public InvalidRecipientException() { }
    public InvalidRecipientException(string message) : base(message) { }
    public InvalidRecipientException(string message, Exception inner) : base(message, inner) { }
}
