using ErrorOr;
using ExpenseTracker.API.Extensions;
using ExpenseTracker.Application.Abstractions.RateLimitingConstants;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Records.Contracts.Requests;
using ExpenseTracker.Application.Records.Contracts.Responses;
using ExpenseTracker.Application.Records.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ExpenseTracker.API.Records.Controllers;

[Route("api/accounts/{userExternalId}/records")]
[ApiController]
public class RecordsController : ControllerBase
{
    private readonly ITransactionRecordService _transactionRecordService;

    public RecordsController(ITransactionRecordService transactionRecordService)
    {
        _transactionRecordService = transactionRecordService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.RecordWrite)]
    public async Task<ActionResult<AddTransactionRecordResponseDto>> Create([FromBody] AddTransactionRecordRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<AddTransactionRecordResponseDto> result = await _transactionRecordService.AddUserTransactionRecord(request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return CreatedAtAction(nameof(Create), new { Id = result.Value.ExternalId }, result.Value);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.RecordRead)]
    public async Task<ActionResult<IEnumerable<GetTransactionRecordResponseDto>>> GetByCategoryId([FromRoute] string userExternalId, [FromQuery] string categoryExternalId, CancellationToken ctoken)
    {
        ErrorOr<IEnumerable<GetTransactionRecordResponseDto>> result = await _transactionRecordService.GetAllUserTransactionsByCategory(userExternalId, categoryExternalId, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.RecordRead)]
    public async Task<ActionResult<IEnumerable<GetTransactionRecordResponseDto>>> GetByCollectionId([FromQuery] string collectionExternalId, CancellationToken ctoken)
    {
        ErrorOr<IEnumerable<GetTransactionRecordResponseDto>> result = await _transactionRecordService.GetAllTransactionsByCollectionId(collectionExternalId, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return Ok(result.Value);
    }

    [HttpPut("{recordExternalId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.RecordWrite)]
    public async Task<ActionResult> Update([FromRoute] string userExternalId, [FromBody] UpdateTransactionRecordRequestDto request, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _transactionRecordService.UpdateUserTransaction(userExternalId, request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return NoContent();
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.RecordWrite)]
    public async Task<ActionResult> UpdateAll([FromRoute] string userExternalId, [FromBody] List<UpdateTransactionRecordRequestDto> request, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _transactionRecordService.UpdateAllUserTransactions(userExternalId, request, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return NoContent();
    }

    [HttpDelete("{recordExternalId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EnableRateLimiting(RateLimitingPolicy.AuthenticatedUsers)]
    [Authorize(Policy = PermissionNames.RecordDelete)]
    public async Task<ActionResult> Delete([FromRoute] string userExternalId, [FromRoute] string recordExternalId, CancellationToken ctoken)
    {
        ErrorOr<int> result = await _transactionRecordService.DeleteTransactionRecord(userExternalId, recordExternalId, ctoken);

        if (result.IsError)
            return result.Errors.MapToStatusCode();

        return NoContent();
    }
}
