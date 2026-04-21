using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Authentication.AuthenticationServices;
using ExpenseTracker.Application.Authentication.Contracts.Request;
using ExpenseTracker.Application.Authentication.Errors;
using ExpenseTracker.Application.Authentication.JwtLib;
using ExpenseTracker.Application.Authorization.BCryptLib;
using ExpenseTracker.Application.Authorization.Tokens.Services;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Authorization.Tokens.Entity;
using ExpenseTracker.Domain.Authorization.Tokens.Repository;
using ExpenseTracker.Domain.Email.Repository;
using FluentAssertions;
using FluentValidation;
using Hangfire;
using Moq;

namespace ExpenseTracker.UnitTests.Authentication;

public class ResetPasswordUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITokenRepository> _tokenRepositoryMock;
    private readonly Mock<IEmailDeliveryRepository> _emailDeliveryRepositoryMock;
    private readonly Mock<IPasswordHistoryRepository> _passwordHistoryRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IJwtTokenValidator> _tokenValidatorMock;
    private readonly Mock<IBackgroundJobClient> _backgroundJobClientMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IDateProvider> _dateProviderMock;
    private readonly Mock<IVerificationTokenObserver> _tokenObserverMock;
    private readonly Mock<IValidator<AddUserRequestDto>> _addUserValidatorMock;
    private readonly Mock<IValidator<LoginRequestDto>> _loginValidatorMock;
    private readonly Mock<IValidator<ResetPassRequestDto>> _resetPasswordValidatorMock;
    private readonly AuthenticationService _sut;

    public ResetPasswordUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenRepositoryMock = new Mock<ITokenRepository>();
        _emailDeliveryRepositoryMock = new Mock<IEmailDeliveryRepository>();
        _passwordHistoryRepositoryMock = new Mock<IPasswordHistoryRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _tokenValidatorMock = new Mock<IJwtTokenValidator>();
        _backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        _tokenServiceMock = new Mock<ITokenService>();
        _dateProviderMock = new Mock<IDateProvider>();
        _tokenObserverMock = new Mock<IVerificationTokenObserver>();
        _addUserValidatorMock = new Mock<IValidator<AddUserRequestDto>>();
        _loginValidatorMock = new Mock<IValidator<LoginRequestDto>>();
        _resetPasswordValidatorMock = new Mock<IValidator<ResetPassRequestDto>>();
        _sut = new AuthenticationService(
            _userRepositoryMock.Object,
            _tokenRepositoryMock.Object,
            _emailDeliveryRepositoryMock.Object,
            _passwordHistoryRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenGeneratorMock.Object,
            _tokenValidatorMock.Object,
            _backgroundJobClientMock.Object,
            _tokenServiceMock.Object,
            _dateProviderMock.Object,
            _tokenObserverMock.Object,
            _addUserValidatorMock.Object,
            _loginValidatorMock.Object,
            _resetPasswordValidatorMock.Object
        );
    }

    [Fact]
    public async Task ResetPassword_WhenEmailTokenIsNullOrEmpty_ShouldReturnInvalidArgsError()
    {
        // Arrange
        string requestToken = string.Empty;
        ResetPassRequestDto request = new ResetPassRequestDto
        {
            Password = "Some-password"
        };

        // Act
        var result = await _sut.ResetPassword(requestToken, request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(AuthenticationErrors.InvalidArgs);
    }

    [Fact]
    public async Task ResetPassword_WhenRequestTokenDoesNotExistOrIsInvalid_ShouldReturnInvalidArgsError()
    {
        // Arrange
        string requestToken = "some-generated-token";
        ResetPassRequestDto request = new ResetPassRequestDto
        {
            Password = "Some-Password"
        };

        Token fakeToken = new Token
        {
            TokenValue = "some-fake-token"
        };

        _tokenRepositoryMock.Setup(
            repo => repo.GetTokenByTokenValue(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(fakeToken);

        // Act
        var result = await _sut.ResetPassword(requestToken, request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(AuthenticationErrors.InvalidArgs);

        _tokenRepositoryMock.Verify(
            repo => repo.GetTokenByTokenValue(
                requestToken,
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ResetPassword_WhenRequestTokenIsAlreadyUsed_ShouldReturnForbiddenError()
    {
        // Arrange
        string requestToken = "some-generated-token";
        ResetPassRequestDto request = new ResetPassRequestDto
        {
            Password = "Password123!"
        };

        Token existingToken = new Token
        {
            TokenValue = requestToken,
            IsUsed = true
        };

        _tokenRepositoryMock.Setup(
            repo => repo.GetTokenByTokenValue(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingToken);

        // Act
        var result = await _sut.ResetPassword(requestToken, request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(AuthenticationErrors.Forbidden);

        _tokenRepositoryMock.Verify(
            repo => repo.GetTokenByTokenValue(
                requestToken,
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ResetPassword_WhenPasswordWasAlreadyUsed_ShouldReturnInvalidPasswordError()
    {
        // Arrange
        string requestToken = "some-generated-token";
        string hashedPassword = "hashedPassword";
        ResetPassRequestDto request = new ResetPassRequestDto
        {
            Password = "Password123!"
        };

        Token existingToken = new Token
        {
            TokenValue = "some-generated-token",
            IsUsed = false,
            TokenUserId = 1
        };

        User existingUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = Guid.NewGuid()
        };
        existingUser.PasswordHistory = new PasswordHistory
        {
            PasswordHash = "hashedPassword"
        };

        _tokenRepositoryMock.Setup(
            repo => repo.GetTokenByTokenValue(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingToken);

        _passwordHasherMock.Setup(hasher => hasher.Hash(
            It.IsAny<string>()))
            .Returns(hashedPassword);

        _userRepositoryMock.Setup(repo => repo.GetUserById(
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _passwordHistoryRepositoryMock.Setup(
            repo => repo.GetByPasswordHash(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser.PasswordHistory);

        // Act
        var result = await _sut.ResetPassword(requestToken, request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(AuthenticationErrors.InvalidPassword);

        _tokenRepositoryMock.Verify(
            repo => repo.GetTokenByTokenValue(
                requestToken,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(
                existingToken.TokenUserId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _passwordHistoryRepositoryMock.Verify(
            repo => repo.GetByPasswordHash(
                hashedPassword,
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ResetPassword_WhenRequestIsValid_ShouldReturnTrue()
    {
        // Arrange
        string requestToken = "some-generated-token";
        string hashedPassword = "newHashedPassword";
        var fixedCreatedAtTimestamp = new DateTimeOffset(2024, 1, 15, 8, 0, 0, TimeSpan.Zero);

        ResetPassRequestDto request = new ResetPassRequestDto
        {
            Password = "Password123!"
        };

        Token existingToken = new Token
        {
            TokenValue = "some-generated-token",
            IsUsed = false,
            TokenUserId = 1
        };

        User existingUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = Guid.NewGuid()
        };

        PasswordHistory passHistory = new PasswordHistory
        {
            UserId = existingUser.Id,
            PasswordHash = hashedPassword,
            CreatedAt = fixedCreatedAtTimestamp
        };

        PasswordHistory? capturedHistory = null;

        _tokenRepositoryMock.Setup(
            repo => repo.GetTokenByTokenValue(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingToken);

        _passwordHasherMock.Setup(
            hasher => hasher.Hash(It.IsAny<string>()))
        .Returns(hashedPassword);

        _userRepositoryMock.Setup(repo => repo.GetUserById(
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _passwordHistoryRepositoryMock.Setup(
            repo => repo.GetByPasswordHash(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync((PasswordHistory?)null);

        _passwordHistoryRepositoryMock.Setup(
            repo => repo.Add(
                It.IsAny<PasswordHistory>(),
                It.IsAny<CancellationToken>()))
        .Callback<PasswordHistory, CancellationToken>((history, _) => capturedHistory = history)
        .ReturnsAsync(1);

        _userRepositoryMock.Setup(
            repo => repo.ApplyBehaviorChanges(
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(It.IsAny<bool>());

        _tokenRepositoryMock.Setup(
            repo => repo.ApplyBehaviorChanges(
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(It.IsAny<bool>());

        // Act
        var result = await _sut.ResetPassword(requestToken, request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();

        capturedHistory?.UserId.Should().Be(existingUser.Id);
        capturedHistory?.PasswordHash.Should().Be(hashedPassword);

        _tokenRepositoryMock.Verify(
            repo => repo.GetTokenByTokenValue(
                requestToken,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(
                existingToken.TokenUserId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _passwordHistoryRepositoryMock.Verify(
            repo => repo.GetByPasswordHash(
                hashedPassword,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _passwordHistoryRepositoryMock.Verify(
            repo => repo.Add(
                It.IsAny<PasswordHistory>(),
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.ApplyBehaviorChanges(
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _tokenRepositoryMock.Verify(
            repo => repo.ApplyBehaviorChanges(
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
