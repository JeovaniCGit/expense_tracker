using ExpenseTracker.Domain.Authorization.Tokens.Entity;
using System.Security.Claims;

namespace ExpenseTracker.Application.Authentication.JwtLib;

public interface IJwtTokenValidator
{
    ClaimsPrincipal Validate(string token);
}
