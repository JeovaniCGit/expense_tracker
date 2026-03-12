using Asp.Versioning;
using ExpenseTracker.Application.Abstractions.RateLimitingConstants;
using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Application.Accounts.Services.AdminServices;
using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace ExpenseTracker.API.Accounts.Controllers;

[ApiVersion("1")]
[Route("api/v{version:apiVersion}/admin/accounts")]
[ApiController]
[ApiExplorerSettings(GroupName = "AdminAccounts")]
[SwaggerTag("Admin endpoints for account management and analytics.")]
public class AdminAccountsController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAdminAnalyticsService _adminAnalyticsService;

    public AdminAccountsController(IUserService userService, IAdminAnalyticsService adminAnalyticsService)
    {
        _userService = userService;
        _adminAnalyticsService = adminAnalyticsService;
    }

    /// <summary>
    /// Retrieves a paginated list of all user accounts.
    /// </summary>
    /// <param name="page">Page number (1-based) for pagination.</param>
    /// <param name="pageSize">Number of users per page.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>A list of user accounts for the requested page.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.Admin)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<GetAllUsersResponseDto>> GetAll([FromRoute] int page, [FromRoute] int pageSize, CancellationToken ctoken)
    {
        IEnumerable<GetAllUsersResponseDto> result = await _userService.GetAllUsers(page, pageSize, ctoken);

        return Ok(result);
    }


    /// <summary>
    /// Retrieves analytics data about users, including statistics and aggregates.
    /// </summary>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>User analytics information.</returns>
    [HttpGet("analytics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.Admin)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<GetUserAnalyticsResponseDto>> GetUserAnalytics(CancellationToken ctoken)
    {
        GetUserAnalyticsResponseDto result = await _adminAnalyticsService.GetUsersAnalytics(ctoken);

        return Ok(result);
    }
}