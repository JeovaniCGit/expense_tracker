using ErrorOr;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Application.Authentication.Contracts.Request;
using ExpenseTracker.Application.Authentication.Contracts.Response;

namespace ExpenseTracker.Application.Authentication.AuthenticationServices;
public interface IAuthenticationService
{
    Task <ErrorOr<AddUserResponseDto>> Register(AddUserRequestDto request, CancellationToken ctoken = default);
    Task<ErrorOr<LoginResponseDto>> Login(LoginRequestDto request, CancellationToken ctoken = default);
    Task<ErrorOr<RefreshResponseDto>> RefreshToken(RefreshRequestDto request, CancellationToken ctoken = default);
    Task<ErrorOr<bool>> ResetPassword(string emailToken, ResetPassRequestDto request, CancellationToken ctoken = default);
    Task <ErrorOr<bool>> ForgotPassword(string userEmail, CancellationToken ctoken = default);
    //string GenerateNewSecurePasswordForReset(CancellationToken ctoken = default);
    Task<ErrorOr<bool>> MarkUserAsVerified(string emailToken, CancellationToken ctoken = default);
}