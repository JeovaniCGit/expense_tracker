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
using FluentValidation;
using Hangfire;
using Moq;

namespace ExpenseTrackerApplication.Tests.Authentication;

public class MarkUserAsVerifiedUseCaseTests
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
    private readonly Mock<IValidator<AddUserRequestDto>> _addUserValidatorMock;
    private readonly Mock<IValidator<LoginRequestDto>> _loginValidatorMock;
    private readonly AuthenticationService _sut;

    public MarkUserAsVerifiedUseCaseTests()
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
        _addUserValidatorMock = new Mock<IValidator<AddUserRequestDto>>();
        _loginValidatorMock = new Mock<IValidator<LoginRequestDto>>();
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
            _addUserValidatorMock.Object,
            _loginValidatorMock.Object
        );
    }

    [Fact]
    public async Task MarkUserAsVerified_WhenTokenDoesNotExistOrIsInvalid_ShouldReturnInvalidArgsError()
    {
        // Arrange
        string requestToken = "some-fake-token";

        Token existingToken = new Token
        {
            TokenValue = "valid-token"
        };

        _tokenRepositoryMock.Setup(repo => repo.GetTokenByTokenValue(requestToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        // Act
        var result = await _sut.MarkUserAsVerified(requestToken, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthenticationErrors.InvalidArgs, result.FirstError);

        _tokenRepositoryMock.Verify(
            repo => repo.GetTokenByTokenValue(requestToken, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task MarkUserAsVerified_WhenTokenWasAlreadyUsed_ShouldReturnInvalidArgsError()
    {
        // Arrange
        string requestToken = "valid-token";

        Token existingToken = new Token
        {
            TokenValue = requestToken,
            IsUsed = true
        };

        _tokenRepositoryMock.Setup(repo => repo.GetTokenByTokenValue(requestToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        // Act
        var result = await _sut.MarkUserAsVerified(requestToken, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthenticationErrors.InvalidArgs, result.FirstError);

        _tokenRepositoryMock.Verify(
            repo => repo.GetTokenByTokenValue(requestToken, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task MarkUserAsVerified_WhenRequestIsValid_ShouldReturnTrue()
    {
        // Arrange
        string requestToken = "valid-token";
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

        Token existingToken = new Token
        {
            TokenValue = requestToken,
            TokenUserId = existingUser.Id,
            IsUsed = false
        };

        _tokenRepositoryMock.Setup(repo => repo.GetTokenByTokenValue(requestToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _userRepositoryMock.Setup(repo => repo.GetUserById(existingToken.TokenUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _userRepositoryMock.Setup(repo => repo.ApplyBehaviorChanges(It.IsAny<CancellationToken>()))
           .ReturnsAsync(It.IsAny<bool>());

        _tokenRepositoryMock.Setup(repo => repo.ApplyBehaviorChanges(It.IsAny<CancellationToken>()))
           .ReturnsAsync(It.IsAny<bool>());

        // Act
        var result = await _sut.MarkUserAsVerified(requestToken, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(typeof(bool), result.Value.GetType());

        _tokenRepositoryMock.Verify(
            repo => repo.GetTokenByTokenValue(requestToken, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.ApplyBehaviorChanges(It.IsAny<CancellationToken>()),
            Times.Once
        );

        _tokenRepositoryMock.Verify(
            repo => repo.ApplyBehaviorChanges(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
