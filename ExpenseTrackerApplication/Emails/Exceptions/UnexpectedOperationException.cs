using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.Emails.Exceptions;

public sealed class UnexpectedOperationException : Exception
{
    public UnexpectedOperationException() { }
    public UnexpectedOperationException(string? message) : base(message) { }
    public UnexpectedOperationException(string? message, Exception? innerException) : base(message, innerException) { }
}
