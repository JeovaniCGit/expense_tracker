using ExpenseTracker.Application.Accounts.Services.UserServices;
using Microsoft.AspNetCore.Http;

namespace ExpenseTracker.Infrastructure.Accounts.CurrentUserService;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _context;

    public CurrentUserService(IHttpContextAccessor context)
    {
        _context = context;
    }

    public Guid UserExternalId =>
        Guid.Parse(_context.HttpContext?.User.Claims
            .FirstOrDefault(c => c.Type == "Sub")?.Value);
}
