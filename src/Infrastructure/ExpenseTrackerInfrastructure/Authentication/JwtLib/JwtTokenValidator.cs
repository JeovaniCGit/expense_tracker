using ExpenseTracker.Application.Authentication.JwtLib;
using ExpenseTracker.Infrastructure.Authentication.JwtLib.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExpenseTracker.Application.Authorization.Tokens.Enums;

namespace ExpenseTracker.Infrastructure.Authentication.JwtLib;

public sealed class JwtTokenValidator : IJwtTokenValidator
{
    private readonly JwtOptions _options;
    private readonly TokenValidationParameters _parameters;

    public JwtTokenValidator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }
    
    private TokenValidationParameters BuildAccessParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,

            ValidateAudience = true,
            ValidAudience = _options.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Convert.FromBase64String(_options.AccessTokenSigningKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    }
    
    private TokenValidationParameters BuildRefreshParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,

            ValidateAudience = true,
            ValidAudience = _options.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Convert.FromBase64String(_options.RefreshTokenSigningKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    }

    public ClaimsPrincipal Validate(string token, TokenDescriptionEnum type)
    {
        var handler = new JwtSecurityTokenHandler();

        var parameters = type switch
        {
            TokenDescriptionEnum.AccessToken => BuildAccessParameters(),
            TokenDescriptionEnum.RefreshToken => BuildRefreshParameters(),
            _ => throw new NotSupportedException($"The {type} token type is not supported. No other types are supported.")
        };

        return handler.ValidateToken(token, parameters, out var validatedToken);
    }
}
