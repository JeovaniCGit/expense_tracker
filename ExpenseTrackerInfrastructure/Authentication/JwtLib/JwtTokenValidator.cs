using ExpenseTracker.Application.Authentication.JwtLib;
using ExpenseTracker.Infrastructure.Authentication.JwtLib.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExpenseTracker.Infrastructure.Authentication.JwtLib;

public sealed class JwtTokenValidator : IJwtTokenValidator
{
    private readonly JwtTokenOptions _options;
    private readonly TokenValidationParameters _parameters;

    public JwtTokenValidator(IOptions<JwtTokenOptions> options)
    {
        _options = options.Value;

        _parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,

            ValidateAudience = true,
            ValidAudience = _options.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.SigningKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    }

    public ClaimsPrincipal Validate(string token)
    {
        var handler = new JwtSecurityTokenHandler();

        return handler.ValidateToken(token, _parameters, out _);
    }
}
