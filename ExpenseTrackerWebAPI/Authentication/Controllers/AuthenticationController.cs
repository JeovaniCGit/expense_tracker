using Asp.Versioning;
using ErrorOr;
using ExpenseTracker.API.Authentication.Cookies;
using ExpenseTracker.API.Extensions;
using ExpenseTracker.Application.Abstractions.RateLimitingConstants;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Application.Authentication.AuthenticationServices;
using ExpenseTracker.Application.Authentication.Contracts.Request;
using ExpenseTracker.Application.Authentication.Contracts.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace ExpenseTracker.API.Authentication.Controllers;

[ApiVersion("1")]
[Route("api/v{version:apiVersion}/auth")]
[ApiController]
[ApiExplorerSettings(GroupName = "Authentication")]
[SwaggerTag("Endpoints for user authentication: register, login, refresh token, password reset and verification.")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly AuthCookieFactory _authCookieFactory;
    public AuthenticationController(IAuthenticationService authenticationService, AuthCookieFactory authCookieFactory)
    {
        _authenticationService = authenticationService;
        _authCookieFactory = authCookieFactory;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">User registration details.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>HTTP 200 OK if registration succeeds.</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AnonymousUser)]
    [AllowAnonymous]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult> Register([FromBody] AddUserRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<AddUserResponseDto> result = await _authenticationService.Register(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return Ok();
    }


    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>Login response including tokens.</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AnonymousUser)]
    [AllowAnonymous]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<LoginResponseDto> result = await _authenticationService.Login(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        Response.Cookies.Append("access_token", result.Value.AccessToken, _authCookieFactory.CreateAccessTokenCookie());
        Response.Cookies.Append("refresh_token", result.Value.RefreshToken, _authCookieFactory.CreateRefreshTokenCookie());

        return Ok(result);
    }


    /// <summary>
    /// Refreshes the user's access token using a refresh token.
    /// </summary>
    /// <param name="request">Refresh token request.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>New access and refresh tokens.</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AnonymousUser)]
    [AllowAnonymous]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<RefreshResponseDto>> Refresh([FromBody] RefreshRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<RefreshResponseDto> result = await _authenticationService.RefreshToken(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        Response.Cookies.Append("access_token", result.Value.AccessToken, _authCookieFactory.CreateAccessTokenCookie());
        Response.Cookies.Append("refresh_token", result.Value.RefreshToken, _authCookieFactory.CreateRefreshTokenCookie());

        return Ok(result);
    }


    /// <summary>
    /// Initiates a password reset process for the specified email.
    /// </summary>
    /// <param name="email">Email of the user requesting a password reset.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>HTTP 200 OK if email sent successfully.</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AnonymousUser)]
    [AllowAnonymous]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult> ForgotPassword([FromQuery] string email, CancellationToken ctoken)
    {
        await _authenticationService.ForgotPassword(email, ctoken);

        return Ok();
    }


    /// <summary>
    /// Resets a user's password using a reset token.
    /// </summary>
    /// <param name="emailToken">Token sent to the user's email.</param>
    /// <param name="request">New password details.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>HTTP 200 OK if password reset succeeds.</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AnonymousUser)]
    [AllowAnonymous]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult> ResetPassword([FromQuery] string emailToken, [FromBody] ResetPassRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<bool> result = await _authenticationService.ResetPassword(emailToken, request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return Ok();
    }


    /// <summary>
    /// Verifies a user's email address using a verification token.
    /// </summary>
    /// <param name="emailToken">Token sent to the user's email.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>HTTP 200 OK if user is successfully verified.</returns>
    [HttpPost("verify-user")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AnonymousUser)]
    [AllowAnonymous]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult> ResetPassword([FromQuery] string emailToken, CancellationToken ctoken)
    {
        ErrorOr<bool> result = await _authenticationService.MarkUserAsVerified(emailToken, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return Ok();
    }
}