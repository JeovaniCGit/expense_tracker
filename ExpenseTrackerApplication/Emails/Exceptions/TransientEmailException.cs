namespace ExpenseTracker.Application.Emails.Exceptions;

public sealed class TransientEmailException : Exception
{
    public TransientEmailException() {}
    public TransientEmailException(string message) : base(message) {}
    public TransientEmailException(string message, Exception inner) : base(message, inner) {}
}
