using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Scheme = "TestAuth";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userExternalId = Request.Headers["X-UserId"].FirstOrDefault() ?? "default-user";
        var roles = Request.Headers["X-UserPerm"].FirstOrDefault()?.Split(',') ?? Array.Empty<string>();

        var claims = new List<Claim>
        {
            new Claim("oid", userExternalId)
        };

        claims.AddRange(roles.Select(r =>
            new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);

        return Task.FromResult(
            AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme))
        );
    }
}