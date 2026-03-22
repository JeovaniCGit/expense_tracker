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

namespace ExpenseTracker.API.Collections.Controllers;

[Route("api/accounts/{userExternalId}/collections")]
[ApiController]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _collectionService;
    public CollectionsController(ICollectionService collectionService)
    {
        _collectionService = collectionService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    [HttpDelete("{collectionExternalId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
