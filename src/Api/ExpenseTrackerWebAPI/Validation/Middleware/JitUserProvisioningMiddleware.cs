using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;

namespace ExpenseTracker.API.Validation.Middleware;

public sealed class JitUserProvisioningMiddleware
{
    private readonly RequestDelegate _next;

    public JitUserProvisioningMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var oid = context.User.FindFirst("oid")?.Value;

            if (oid is not null)
            {
                Guid externalId = Guid.Parse(oid);
                User? existingUser = await userRepository.GetUserByExternalId(externalId);

                if (existingUser is null)
                {
                    await userRepository.CreateUser(new User
                    {
                        ExternalId = externalId,
                        Firstname = context.User.FindFirst("first_name")?.Value ?? "",
                        Lastname = context.User.FindFirst("last_name")?.Value ?? "",
                        Email = context.User.FindFirst("preferred_username")?.Value ?? "",
                    });
                }
            }
        }

        await _next(context);
    }
}