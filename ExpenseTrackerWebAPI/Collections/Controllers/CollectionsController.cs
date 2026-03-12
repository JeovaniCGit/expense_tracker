using Asp.Versioning;
using ErrorOr;
using ExpenseTracker.API.Extensions;
using ExpenseTracker.Application.Abstractions.RateLimitingConstants;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Collections.Contracts.Requests;
using ExpenseTracker.Application.Collections.Contracts.Responses;
using ExpenseTracker.Application.Collections.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace ExpenseTracker.API.Collections.Controllers;

[ApiVersion("1")]
[Route("api/v{version:apiVersion}/accounts/{userExternalId}/collections")]
[ApiController]
[ApiExplorerSettings(GroupName = "Collections")]
[SwaggerTag("Manage user collections for transaction grouping.")]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _collectionService;
    public CollectionsController(ICollectionService collectionService)
    {
        _collectionService = collectionService;
    }

    /// <summary>
    /// Creates a new collection for grouping user transactions.
    /// </summary>
    /// <param name="request">Details of the collection to create.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>The newly created collection.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.CollectionWrite)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<AddCollectionResponseDto>> Create([FromBody] AddCollectionRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<AddCollectionResponseDto> result = await _collectionService.AddCollection(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return CreatedAtAction(nameof(Create), new { Id = result.Value.ExternalId }, result.Value);
    }


    /// <summary>
    /// Retrieves all collections for the authenticated user, optionally filtered by date range.
    /// </summary>
    /// <param name="startDate">Optional start date filter.</param>
    /// <param name="endDate">Optional end date filter.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>List of user collections.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.CollectionRead)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<IEnumerable<GetCollectionResponseDto>>> Get([FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate, CancellationToken ctoken)
    {
        ErrorOr<IEnumerable<GetCollectionResponseDto>> result = await _collectionService.GetAllUserCollections(startDate?? null, endDate?? null, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return Ok(result.Value);
    }


    /// <summary>
    /// Updates an existing collection.
    /// </summary>
    /// <param name="request">Collection update details.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>No content if update succeeds.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.CollectionWrite)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult> Update([FromBody] UpdateCollectionRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _collectionService.UpdateCollection(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return NoContent();
    }


    /// <summary>
    /// Deletes a collection by its external ID.
    /// </summary>
    /// <param name="collectionExternalId">External ID of the collection to delete.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>No content if deletion succeeds.</returns>
    [HttpDelete("{collectionExternalId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.CollectionDelete)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult> Delete([FromRoute] string collectionExternalId, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _collectionService.DeleteCollection(collectionExternalId, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return NoContent();
    }
} 