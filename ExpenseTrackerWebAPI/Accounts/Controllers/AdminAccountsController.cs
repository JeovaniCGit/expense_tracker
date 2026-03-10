using ExpenseTracker.Application.Abstractions.RateLimitingConstants;
using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Application.Accounts.Services.AdminServices;
using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ExpenseTracker.API.Accounts.Controllers;

[Route("api/admin/accounts")]
[ApiController]
public class AdminAccountsController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAdminAnalyticsService _adminAnalyticsService;

    public AdminAccountsController(IUserService userService, IAdminAnalyticsService adminAnalyticsService)
    {
        _userService = userService;
        _adminAnalyticsService = adminAnalyticsService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.Admin)]
    public async Task<ActionResult<GetAllUsersResponseDto>> GetAll([FromRoute] int page, [FromRoute] int pageSize, CancellationToken ctoken)
    {
        IEnumerable<GetAllUsersResponseDto> result = await _userService.GetAllUsers(page, pageSize, ctoken);

        return Ok(result);
    }

    [HttpGet("analytics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.Admin)]
    public async Task<ActionResult<GetUserAnalyticsResponseDto>> GetUserAnalytics(CancellationToken ctoken)
    {
        GetUserAnalyticsResponseDto result = await _adminAnalyticsService.GetUsersAnalytics(ctoken);

        return Ok(result);
    }
}
