using ExpenseTracker.Application.Categories.Contracts.Responses;
using ExpenseTracker.Application.Records.Contracts.Responses;

namespace ExpenseTracker.Application.Accounts.Contracts.Responses;
public sealed record GetUserResponseDto
{
    public required Guid UserExternalId { get; set; }
    public required string Firstname { get; init; }
    public required string Lastname { get; init; }
    public required string Email { get; init; }

    public IEnumerable<GetTransactionRecordResponseDto> Transactions { get; init; } = Array.Empty<GetTransactionRecordResponseDto>();

    public IEnumerable<GetTransactionRecordCategoryResponseDto> Categories { get; init; } = Array.Empty<GetTransactionRecordCategoryResponseDto>();
}
