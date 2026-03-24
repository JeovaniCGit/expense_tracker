using Asp.Versioning;
using ErrorOr;
using ExpenseTracker.API.Extensions;
using ExpenseTracker.Application.Abstractions.RateLimitingConstants;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Records.Contracts.Requests;
using ExpenseTracker.Application.Records.Contracts.Responses;
using ExpenseTracker.Application.Records.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace ExpenseTracker.API.Records.Controllers;

[ApiVersion("1")]
[Route("api/v{version:apiVersion}/accounts/{userExternalId}/records")]
[ApiController]
[ApiExplorerSettings(GroupName = "Records")]
[SwaggerTag("Manage transaction records for a user (create, query, update, delete).")]
public class RecordsController : ControllerBase
{
    private readonly ITransactionRecordService _transactionRecordService;

    public RecordsController(ITransactionRecordService transactionRecordService)
    {
        _transactionRecordService = transactionRecordService;
    }

    /// <summary>
    /// Creates a new transaction record for the authenticated user.
    /// </summary>
    /// <param name="request">Transaction record creation details.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>The created transaction record.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.RecordWrite)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<AddTransactionRecordResponseDto>> Create([FromBody] AddTransactionRecordRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<AddTransactionRecordResponseDto> result = await _transactionRecordService.AddUserTransactionRecord(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return CreatedAtAction(nameof(Create), new { Id = result.Value.ExternalId }, result.Value);
    }


    /// <summary>
    /// Retrieves all transaction records for the authenticated user filtered by category.
    /// </summary>
    /// <param name="categoryExternalId">External ID of the category.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>List of transaction records under the specified category.</returns>
    [HttpGet("by-category")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.RecordRead)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<IEnumerable<GetTransactionRecordResponseDto>>> GetByCategoryId([FromQuery] string categoryExternalId, CancellationToken ctoken)
    {
        ErrorOr<IEnumerable<GetTransactionRecordResponseDto>> result = await _transactionRecordService.GetAllUserTransactionsByCategory(categoryExternalId, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return Ok(result.Value);
    }


    /// <summary>
    /// Retrieves all transaction records for the authenticated user filtered by collection.
    /// </summary>
    /// <param name="collectionExternalId">External ID of the collection.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>List of transaction records under the specified collection.</returns>
    [HttpGet("by-collection")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.RecordRead)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult<IEnumerable<GetTransactionRecordResponseDto>>> GetByCollectionId([FromQuery] string collectionExternalId, CancellationToken ctoken)
    {
        ErrorOr<IEnumerable<GetTransactionRecordResponseDto>> result = await _transactionRecordService.GetAllTransactionsByCollectionId(collectionExternalId, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return Ok(result.Value);
    }


    /// <summary>
    /// Updates a single transaction record by external ID.
    /// </summary>
    /// <param name="request">Transaction record update details.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>No content if update succeeds.</returns>
    [HttpPut("{recordExternalId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.RecordWrite)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult> Update([FromBody] UpdateTransactionRecordRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _transactionRecordService.UpdateUserTransaction(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return NoContent();
    }


    /// <summary>
    /// Updates multiple transaction records in bulk.
    /// </summary>
    /// <param name="request">List of transaction record updates.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>No content if all updates succeed.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.RecordWrite)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult> UpdateAll([FromBody] List<UpdateTransactionRecordRequestDto> request, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _transactionRecordService.UpdateAllUserTransactions(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return NoContent();
    }


    /// <summary>
    /// Deletes a transaction record by its external ID.
    /// </summary>
    /// <param name="recordExternalId">External ID of the record to delete.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>No content if deletion succeeds.</returns>
    [HttpDelete("{recordExternalId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.RecordDelete)]
    [RequestTimeout("FastOperation")]
    public async Task<ActionResult> Delete([FromRoute] string recordExternalId, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _transactionRecordService.DeleteTransactionRecord(recordExternalId, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return NoContent();
    }
}