using Asp.Versioning;
using ErrorOr;
using ExpenseTracker.API.Extensions;
using ExpenseTracker.Application.Abstractions.RateLimitingConstants;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Validation.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace ExpenseTracker.API.Accounts.Controllers;

[ApiVersion("1")]
[Route("api/v{version:apiVersion}/accounts")]
[ApiController]
[ApiExplorerSettings(GroupName = "Accounts")]
[SwaggerTag("Endpoints to manage user accounts (create, read, update, delete).")]
public class AccountsController : ControllerBase
{
    private readonly IUserService _userService;

    public AccountsController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Retrieves a user account by its external ID.
    /// </summary>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>User account details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetUserResponseDto), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
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


    /// <summary>
    /// Updates an existing user account with the provided details.
    /// </summary>
    /// <param name="request">User update details.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>No content if update is successful.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(ApiErrorResponse), 204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
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


    /// <summary>
    /// Deletes a user account identified by the external ID.
    /// </summary>
    /// <param name="externalId">External ID of the user to delete.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>No content if deletion is successful.</returns>
    [HttpDelete]
    [ProducesResponseType(typeof(ApiErrorResponse), 204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
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