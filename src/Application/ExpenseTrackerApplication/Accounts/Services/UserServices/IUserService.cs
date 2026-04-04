using ErrorOr;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Contracts.Responses;

namespace ExpenseTracker.Application.Accounts.Services.UserServices;

public interface IUserService
{
    Task<ErrorOr<AddUserResponseDto>> CreateUser(AddUserRequestDto request, CancellationToken ctoken = default);
    Task<ErrorOr<int>> UpdateUser(UpdateUserRequestDto request, CancellationToken ctoken = default);
    Task<ErrorOr<int>> DeleteUser(string externalId, CancellationToken ctoken = default);
    Task<ErrorOr<GetUserResponseDto>> GetUserByExternalId(CancellationToken ctoken = default);
    Task<IEnumerable<GetAllUsersResponseDto>> GetAllUsers(int page, int pageSize, CancellationToken ctoken = default);
}
