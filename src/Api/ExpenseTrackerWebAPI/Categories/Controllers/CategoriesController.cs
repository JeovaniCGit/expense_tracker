using Asp.Versioning;
using ErrorOr;
using ExpenseTracker.API.Extensions;
using ExpenseTracker.Application.Abstractions.RateLimitingConstants;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Categories.Contracts.Requests;
using ExpenseTracker.Application.Categories.Contracts.Responses;
using ExpenseTracker.Application.Categories.Services;
using ExpenseTracker.Application.Validation.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace ExpenseTracker.API.Categories.Controllers;

[ApiVersion("1")]
[Route("api/v{version:apiVersion}/accounts/me/records/categories")]
[ApiController]
[ApiExplorerSettings(GroupName = "Categories")]
[SwaggerTag("Manage transaction record categories for a user.")]
public class CategoriesController : ControllerBase
{
    private readonly ITransactionRecordCategoryService _categoryService;

    public CategoriesController(ITransactionRecordCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Creates a new transaction record category for the authenticated user.
    /// </summary>
    /// <param name="request">Category creation details.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>The newly created category.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AddTransactionRecordCategoryResponseDto), 201)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.CategoryWrite)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<AddTransactionRecordCategoryResponseDto>> Create([FromBody] AddTransactionRecordCategoryRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<AddTransactionRecordCategoryResponseDto> result = await _categoryService.AddUserTransactionRecordCategory(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return CreatedAtAction(nameof(Create), new { Id = result.Value.CategoryExternalId }, result.Value);
    }


    /// <summary>
    /// Retrieves all transaction record categories for the authenticated user.
    /// </summary>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>List of transaction record categories.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GetTransactionRecordCategoryResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.CategoryRead)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<IEnumerable<GetTransactionRecordCategoryResponseDto>>> GetAllByUserId(CancellationToken ctoken)
    {
        ErrorOr<IEnumerable<GetTransactionRecordCategoryResponseDto>> result = await _categoryService.GetAllUserTransactionCategories(ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return Ok(result.Value);
    }


    /// <summary>
    /// Updates a single transaction record category by external ID.
    /// </summary>
    /// <param name="request">Category update details.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>No content if the update succeeds.</returns>
    [HttpPut("{categoryExternalId}")]
    [ProducesResponseType(typeof(ApiErrorResponse), 204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.CategoryWrite)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<UpdateTransactionRecordCategoryResponseDto>> Update([FromBody] UpdateTransactionRecordCategoryRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _categoryService.UpdateUserTransactionCategory(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return NoContent();
    }


    /// <summary>
    /// Updates multiple transaction record categories in bulk.
    /// </summary>
    /// <param name="request">List of category updates.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>No content if all updates succeed.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(ApiErrorResponse), 204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.CategoryWrite)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<UpdateTransactionRecordCategoryResponseDto>> UpdateAll([FromBody] List<UpdateTransactionRecordCategoryRequestDto> request, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _categoryService.UpdateAllUserTransactionCategories(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return NoContent();
    }


    /// <summary>
    /// Deletes a transaction record category by external ID.
    /// </summary>
    /// <param name="categoryExternalId">External ID of the category to delete.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>No content if deletion succeeds.</returns>
    [HttpDelete("{categoryExternalId}")]
    [ProducesResponseType(typeof(ApiErrorResponse), 204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.CategoryDelete)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult> Delete([FromRoute] string categoryExternalId, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _categoryService.DeleteTransactionRecordCategory(categoryExternalId, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return NoContent();
    }
}