using ErrorOr;
using ExpenseTracker.API.Extensions;
using ExpenseTracker.Application.Abstractions.RateLimitingConstants;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ExpenseTracker.API.Accounts.Controllers;

[Route("api/accounts")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly IUserService _userService;

    public AccountsController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EnableRateLimiting(RateLimitingPolicy.AnonymousUser)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<AddUserResponseDto>> Create([FromBody] AddUserRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<AddUserResponseDto> result = await _userService.CreateUser(request, ctoken);

        if (result.IsError)
        {
            return result.Errors.MapToStatusCode();
        }

        return CreatedAtAction(nameof(Create), new { Id = result.Value.ExternalId }, result.Value);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.UserRead)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<GetUserResponseDto>> GetById(CancellationToken ctoken)
    {
        ErrorOr<GetUserResponseDto> result = await _userService.GetUserByExternalId(ctoken);

        if (result.IsError)
        {
            return result.Errors.MapToStatusCode();
        }

        return result.Value;
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.UserWrite)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<UpdateUserResponseDto>> Update([FromBody] UpdateUserRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _userService.UpdateUser(request, ctoken);

        if (result.IsError)
        {
            return result.Errors.MapToStatusCode();
        }

        return NoContent();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.UserDelete)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<bool>> Delete([FromRoute] string externalId, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _userService.DeleteUser(externalId, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return NoContent();
    }
}
