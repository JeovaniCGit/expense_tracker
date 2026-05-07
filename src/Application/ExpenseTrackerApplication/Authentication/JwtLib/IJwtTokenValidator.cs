using ExpenseTracker.Domain.Authorization.Tokens.Entity;
using System.Security.Claims;
using ExpenseTracker.Application.Authorization.Tokens.Enums;

namespace ExpenseTracker.Application.Authentication.JwtLib;

public interface IJwtTokenValidator
{
    ClaimsPrincipal Validate(string token, TokenDescriptionEnum type);
}
