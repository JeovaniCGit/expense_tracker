using ErrorOr;
using ExpenseTracker.API.Extensions;
using ExpenseTracker.Application.Abstractions.RateLimitingConstants;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Categories.Contracts.Requests;
using ExpenseTracker.Application.Categories.Contracts.Responses;
using ExpenseTracker.Application.Categories.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ExpenseTracker.API.Categories.Controllers;

[Route("api/accounts/{userExternalId}/records/categories")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ITransactionRecordCategoryService _categoryService;

    public CategoriesController(ITransactionRecordCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.CategoryWrite)]
    [RequestTimeout("FastRead")]
    public async Task<ActionResult<AddTransactionRecordCategoryResponseDto>> Create([FromBody] AddTransactionRecordCategoryRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<AddTransactionRecordCategoryResponseDto> result = await _categoryService.AddUserTransactionRecordCategory(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return CreatedAtAction(nameof(Create), new { Id = result.Value.CategoryExternalId }, result.Value);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.CategoryRead)]
    [RequestTimeout("FastRead")]
    public async Task<ActionResult<IEnumerable<GetTransactionRecordCategoryResponseDto>>> GetAllByUserId(CancellationToken ctoken)
    {
        ErrorOr<IEnumerable<GetTransactionRecordCategoryResponseDto>> result = await _categoryService.GetAllUserTransactionCategories(ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return Ok(result.Value);
    }

    [HttpPut("{categoryExternalId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.CategoryWrite)]
    [RequestTimeout("FastRead")]
    public async Task<ActionResult<UpdateTransactionRecordCategoryResponseDto>> Update([FromBody] UpdateTransactionRecordCategoryRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _categoryService.UpdateUserTransactionCategory(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return NoContent();
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.CategoryWrite)]
    [RequestTimeout("FastRead")]
    public async Task<ActionResult<UpdateTransactionRecordCategoryResponseDto>> UpdateAll([FromBody] List<UpdateTransactionRecordCategoryRequestDto> request, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _categoryService.UpdateAllUserTransactionCategories(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return NoContent();
    }

    [HttpDelete("{categoryExternalId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.CategoryDelete)]
    [RequestTimeout("FastRead")]
    public async Task<ActionResult> Delete([FromQuery] string categoryExternalId, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _categoryService.DeleteTransactionRecordCategory(categoryExternalId, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return NoContent();
    }
}
