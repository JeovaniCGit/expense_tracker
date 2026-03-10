using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.Emails.Exceptions;

public sealed class EmailServiceUnavailableException: Exception
{
    public EmailServiceUnavailableException() { }
    public EmailServiceUnavailableException(string message) : base(message) { }
    public EmailServiceUnavailableException(string message, Exception inner) : base(message, inner) { }
}
