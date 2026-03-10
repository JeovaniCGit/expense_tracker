
using ErrorOr;
using ExpenseTracker.API.Extensions;
using ExpenseTracker.Application.Abstractions.RateLimitingConstants;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Application.Authentication.AuthenticationServices;
using ExpenseTracker.Application.Authentication.Contracts.Request;
using ExpenseTracker.Application.Authentication.Contracts.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ExpenseTracker.API.Authentication.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EnableRateLimiting(RateLimitingPolicy.AnonymousUser)]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromBody] AddUserRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<AddUserResponseDto> result = await _authenticationService.Register(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return Ok();
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting(RateLimitingPolicy.AnonymousUser)]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<LoginResponseDto> result = await _authenticationService.Login(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return Ok(result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting(RateLimitingPolicy.AnonymousUser)]
    [AllowAnonymous]
    public async Task<ActionResult<RefreshResponseDto>> Refresh([FromBody] RefreshRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<RefreshResponseDto> result = await _authenticationService.RefreshToken(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting(RateLimitingPolicy.AnonymousUser)]
    [AllowAnonymous]
    public async Task<ActionResult> ForgotPassword([FromQuery] string email, CancellationToken ctoken)
    {
        await _authenticationService.ForgotPassword(email, ctoken);

        return Ok();
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting(RateLimitingPolicy.AnonymousUser)]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword([FromQuery] string emailToken, [FromBody] ResetPassRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<bool> result = await _authenticationService.ResetPassword(emailToken, request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return Ok();
    }

    [HttpPost("verify-user")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting(RateLimitingPolicy.AnonymousUser)]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword([FromQuery] string emailToken,CancellationToken ctoken)
    {
        ErrorOr<bool> result = await _authenticationService.MarkUserAsVerified(emailToken, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return Ok();
    }
}
