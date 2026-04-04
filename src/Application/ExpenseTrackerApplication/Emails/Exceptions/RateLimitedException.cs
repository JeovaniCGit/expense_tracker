using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.Emails.Exceptions;

public sealed class RateLimitedException : Exception
{
    public RateLimitedException() { }
    public RateLimitedException(string message) : base(message) { }
    public RateLimitedException(string message, Exception inner) : base(message, inner) { }
}
