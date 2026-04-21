using ErrorOr;
using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Application.Authentication.Contracts.Request;
using ExpenseTracker.Application.Authentication.Contracts.Response;
using ExpenseTracker.Application.Authentication.Errors;
using ExpenseTracker.Application.Authentication.JwtLib;
using ExpenseTracker.Application.Authorization.BCryptLib;
using ExpenseTracker.Application.Authorization.Tokens.Enums;
using ExpenseTracker.Application.Authorization.Tokens.Services;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Application.Emails.Enums;
using ExpenseTracker.Application.Emails.Jobs;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Authorization.Tokens.Entity;
using ExpenseTracker.Domain.Authorization.Tokens.Repository;
using ExpenseTracker.Domain.Email.Entity;
using ExpenseTracker.Domain.Email.Repository;
using FluentValidation;
using Hangfire;
using System.IdentityModel.Tokens.Jwt;


namespace ExpenseTracker.Application.Authentication.AuthenticationServices;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly IEmailDeliveryRepository _emailDeliveryRepository;
    private readonly IPasswordHistoryRepository _passwordHistoryRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IJwtTokenValidator _tokenValidator;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ITokenService _tokenService;
    private readonly IDateProvider _dateProvider;
    private readonly IVerificationTokenObserver _tokenObserver;
    private readonly IValidator<AddUserRequestDto> _addUserValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<ResetPassRequestDto> _resetPasswordValidator;

    public AuthenticationService(
        IUserRepository userRepository,
        ITokenRepository tokenRepository,
        IEmailDeliveryRepository emailDeliveryRepository,
        IPasswordHistoryRepository passwordHistoryRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IJwtTokenValidator tokenValidator,
        IBackgroundJobClient backgroundJobClient,
        ITokenService tokenService,
        IDateProvider dateProvider,
        IVerificationTokenObserver tokenObserver,
        IValidator<AddUserRequestDto> addUserValidator,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<ResetPassRequestDto> resetPasswordValidator
        )
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _emailDeliveryRepository = emailDeliveryRepository;
        _passwordHistoryRepository = passwordHistoryRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _tokenValidator = tokenValidator;
        _backgroundJobClient = backgroundJobClient;
        _tokenService = tokenService;
        _dateProvider = dateProvider;
        _tokenObserver = tokenObserver;
        _addUserValidator = addUserValidator;
        _loginValidator = loginValidator;
        _resetPasswordValidator = resetPasswordValidator;
    }

    public async Task<ErrorOr<bool>> ForgotPassword(string userEmail, CancellationToken ctoken = default)
    {
        if (string.IsNullOrEmpty(userEmail))
            return AuthenticationErrors.InvalidArgs;

        User? existingUser = await _userRepository.GetUserByEmail(userEmail, ctoken);

        if (existingUser is null)
            return AuthenticationErrors.InvalidArgs;

        Token verificationToken = _tokenService.GenerateToken(TokenDescriptionEnum.PasswordResetToken, existingUser.Id, ctoken);

        await _tokenService.AddToken(verificationToken, ctoken);
        ScheduleResetPassEmail(userEmail, existingUser.Firstname, verificationToken.TokenValue, existingUser.Id);
        EmailDelivery emailStatus = new EmailDelivery
        {
            UserId = existingUser.Id,
            Status = EmailDeliveryStatus.Verification.ToString(),
            SentAt = null
        };
        _emailDeliveryRepository.Add(emailStatus);

        return true;
    }

    //public string GenerateNewSecurePasswordForReset(CancellationToken ctoken = default)
    //{
    //    string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    //    string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    //    string Numbers = "0123456789";
    //    string Special = "!@#$%^&*()-_=+[]{}|;:,.<>?";

    //    string allChars = Uppercase + Lowercase + Numbers + Special;

    //    Random rand = new Random();
    //    int minimumLength = 16;
    //    string newSecurePassword = "";

    //    for (int i = 0; i < minimumLength; i++)
    //    {
    //        int index = rand.Next(allChars.Length);
    //        newSecurePassword += allChars[index];
    //    }

    //    return newSecurePassword;
    //}

    public async Task<ErrorOr<AddUserResponseDto>> Register(AddUserRequestDto request, CancellationToken ctoken = default)
    {
        await _addUserValidator.ValidateAndThrowAsync(request, ctoken);

        User? existingUser = await _userRepository.GetUserByEmail(request.Email, ctoken);

        if (existingUser is not null)
            return AuthenticationErrors.DuplicatedEntry;

        User mappedRecord = new User
        {
            Firstname = request.Firstname,
            Lastname = request.Lastname,
            Email = request.Email,
            Password = _passwordHasher.Hash(request.Password),
            RoleId = (long)UserRoleEnum.RegularUser
        };

        User addedRecord = await _userRepository.CreateUser(mappedRecord, ctoken);
        Token verificationToken = _tokenService.GenerateToken(TokenDescriptionEnum.EmailVerificationToken, addedRecord.Id, ctoken);
        await _tokenRepository.AddToken(verificationToken, ctoken);

        _tokenObserver.OnTokenGenerated(addedRecord.Email, verificationToken.TokenValue);

        ScheduleVerificationEmail(addedRecord.Email, addedRecord.Firstname, verificationToken.TokenValue, addedRecord.Id);
        EmailDelivery emailStatus = new EmailDelivery
        {
            UserId = addedRecord.Id,
            Status = EmailDeliveryStatus.Verification.ToString(),
            SentAt = null
        };

        _emailDeliveryRepository.Add(emailStatus);

        return new AddUserResponseDto
        {
            ExternalId = addedRecord.ExternalId,
            Firstname = addedRecord.Firstname,
            Lastname = addedRecord.Lastname,
            CreatedAt = addedRecord.CreatedAt
        };
    }

    public async Task<ErrorOr<LoginResponseDto>> Login(LoginRequestDto request, CancellationToken ctoken = default)
    {
        await _loginValidator.ValidateAndThrowAsync(request, ctoken);

        User? existingUser = await _userRepository.GetUserByEmail(request.Email, ctoken);

        if (existingUser is null)
            return AuthenticationErrors.InvalidArgs;

        if (!_passwordHasher.Verify(request.Password, existingUser.Password))
            return AuthenticationErrors.InvalidArgs;

        List<string> userPermissions = existingUser.Role.RolePermissions
            .Select(rp => rp.Permission.PermissionName)
            .ToList();

        string accessToken = _tokenGenerator.GenerateAccessToken(existingUser.ExternalId, userPermissions, existingUser.Email, ctoken);
        string refreshToken = _tokenGenerator.GenerateRefreshToken(existingUser.ExternalId, existingUser.Email, ctoken);

        Token newRecord = new Token
        {
            TokenValue = refreshToken,
            TokenTypeId = (long)TokenDescriptionEnum.RefreshToken,
            TokenUserId = existingUser.Id
        };

        await _tokenService.AddToken(newRecord, ctoken);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<ErrorOr<RefreshResponseDto>> RefreshToken(RefreshRequestDto request, CancellationToken ctoken = default)
    {
        var principal = _tokenValidator.Validate(request.RefreshToken);

        if (principal.FindFirst(JwtRegisteredClaimNames.Typ)!.Value != TokenDescriptionEnum.RefreshToken.ToString())
            return AuthenticationErrors.Unauthorized;

        Guid userExternalId = Guid.Parse(principal.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
        User? existingUser = await _userRepository.GetUserByExternalId(userExternalId, ctoken);

        List<string> userPermissions = existingUser.Role.RolePermissions
            .Select(rp => rp.Permission.PermissionName)
            .ToList();

        string AccessToken = _tokenGenerator.GenerateAccessToken(existingUser.ExternalId, userPermissions, existingUser.Email, ctoken);
        string RefreshToken = _tokenGenerator.GenerateRefreshToken(existingUser.ExternalId, existingUser.Email, ctoken);

        Token? existingToken = await _tokenRepository.GetTokenByTokenValue(request.RefreshToken, ctoken);

        if (existingToken is null || !Equals(request.RefreshToken ?? "falseToken", existingToken.TokenValue))
            return AuthenticationErrors.InvalidArgs;

        if (existingToken.IsUsed)
            return AuthenticationErrors.InvalidArgs;

        existingToken.IsUsed = true;
        existingToken.UsedAt = _dateProvider.Now;

        Token newRecord = new Token
        {
            TokenValue = RefreshToken,
            TokenTypeId = (long)TokenDescriptionEnum.RefreshToken,
            TokenUserId = existingUser.Id
        };

        await _tokenRepository.AddToken(newRecord, ctoken);

        return new RefreshResponseDto
        {
            AccessToken = AccessToken,
            RefreshToken = RefreshToken
        };
    }

    public async Task<ErrorOr<bool>> ResetPassword(string emailToken, ResetPassRequestDto request, CancellationToken ctoken = default)
    {
        await _resetPasswordValidator.ValidateAndThrowAsync(request, ctoken);

        if (string.IsNullOrEmpty(emailToken))
            return AuthenticationErrors.InvalidArgs;

        Token? existingToken = await _tokenRepository.GetTokenByTokenValue(emailToken, ctoken);

        if (existingToken is null || !Equals(emailToken, existingToken.TokenValue))
            return AuthenticationErrors.InvalidArgs;

        if (existingToken.IsUsed)
            return AuthenticationErrors.Forbidden;

        string requestHashedPass = _passwordHasher.Hash(request.Password);

        User? existingUser = await _userRepository.GetUserById(existingToken.TokenUserId, ctoken);
        PasswordHistory? passwordHistory = await _passwordHistoryRepository.GetByPasswordHash(requestHashedPass, ctoken);

        if (passwordHistory is not null)
            return AuthenticationErrors.InvalidPassword;

        PasswordHistory newPasswordHistory = new PasswordHistory
        {
            UserId = existingUser!.Id,
            PasswordHash = requestHashedPass,
            CreatedAt = _dateProvider.Now
        };

        await _passwordHistoryRepository.Add(newPasswordHistory, ctoken);
        existingToken.IsUsed = true;
        existingToken.UsedAt = _dateProvider.Now;
        existingUser.PasswordLastUpdated = _dateProvider.Now;

        await _userRepository.ApplyBehaviorChanges(ctoken);
        await _tokenRepository.ApplyBehaviorChanges(ctoken);

        return true;
    }

    public async Task<ErrorOr<bool>> MarkUserAsVerified(string emailToken, CancellationToken ctoken = default)
    {
        Token? existingToken = await _tokenRepository.GetTokenByTokenValue(emailToken, ctoken);

        if (existingToken is null || !Equals(emailToken ?? "falseToken", existingToken.TokenValue))
            return AuthenticationErrors.InvalidArgs;

        if (existingToken.IsUsed)
            return AuthenticationErrors.InvalidArgs;

        User? existingUser = await _userRepository.GetUserById(existingToken.TokenUserId, ctoken);

        existingUser!.IsEmailVerified = true;
        existingToken.IsUsed = true;
        existingToken.UsedAt = _dateProvider.Now;

        await _userRepository.ApplyBehaviorChanges(ctoken);
        await _tokenRepository.ApplyBehaviorChanges(ctoken);

        return true;
    }

    private void ScheduleVerificationEmail(string userEmail, string username, string token, long userId)
    {
        _backgroundJobClient.Enqueue<SendEmailJob>(job => job.RetryVerificationEmail(userEmail, username, token, userId, default!));
    }

    private void ScheduleResetPassEmail(string userEmail, string username, string token, long userId)
    {
        _backgroundJobClient.Enqueue<SendEmailJob>(job => job.RetryPasswordResetEmail(userEmail, username, token, userId, default!));
    }
}
