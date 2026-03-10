using ExpenseTracker.Application.Authentication.JwtLib;
using ExpenseTracker.Application.Authorization.Tokens.Enums;
using ExpenseTracker.Infrastructure.Authentication.JwtLib.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExpenseTracker.Infrastructure.Authentication.JwtLib;
public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtTokenOptions _options;

    public JwtTokenGenerator(IOptions<JwtTokenOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateAccessToken(Guid userExternalId, List<string> userPermissions, string email, CancellationToken ctoken = default)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(_options.SigningKey);

        List<Claim> claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, userExternalId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Typ, TokenDescriptionEnum.AccessToken.ToString())
        };

        foreach (string perm in userPermissions)
        {
            claims.Add(new Claim("Permission", perm));  
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_options.AccessTokenExpiryMinutes)),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken(Guid userExternalId, string email, CancellationToken ctoken = default)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(_options.SigningKey);

        List<Claim> claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, userExternalId.ToString()),
            new Claim(JwtRegisteredClaimNames.Typ, TokenDescriptionEnum.RefreshToken.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(Convert.ToInt32(_options.RefreshTokenExpiryDays)),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
