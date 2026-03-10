using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Authentication.AuthenticationServices;
using ExpenseTracker.Application.Authentication.Contracts.Request;
using ExpenseTracker.Application.Authentication.Errors;
using ExpenseTracker.Application.Authentication.JwtLib;
using ExpenseTracker.Application.Authorization.BCryptLib;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Authorization.Tokens.Services;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Authorization.Perms.Entity;
using ExpenseTracker.Domain.Authorization.RolePerms.Entity;
using ExpenseTracker.Domain.Authorization.Tokens.Entity;
using ExpenseTracker.Domain.Authorization.Tokens.Repository;
using ExpenseTracker.Domain.Authorization.UserRoles.Entity;
using ExpenseTracker.Domain.Email.Repository;
using FluentValidation;
using Hangfire;
using Moq;

namespace ExpenseTrackerApplication.Tests.Authentication;

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
    private readonly Mock<IValidator<AddUserRequestDto>> _addUserValidatorMock;
    private readonly Mock<IValidator<LoginRequestDto>> _loginValidatorMock;
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
    public async Task ResetPassword_WhenEmailTokenOrRequestPasswordIsNullOrEmpty_ShouldReturnInvalidArgsError()
    {
        // Arrange
        string requestToken = string.Empty;
        ResetPassRequestDto request = new ResetPassRequestDto
        {
            Password = string.Empty
        };

        // Act
        var result = await _sut.ResetPassword(requestToken, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthenticationErrors.InvalidArgs, result.FirstError);
    }

    [Fact]
    public async Task ResetPassword_WhenRequestTokenDoesNotExistOrIsInvalid_ShouldReturnInvalidArgsError()
    {
        // Arrange
        string requestToken = "some-generated-token";
        ResetPassRequestDto request = new ResetPassRequestDto
        {
            Password = "Password123!"
        };

        Token fakeToken = new Token
        {
            TokenValue = "some-fake-token"
        };

        _tokenRepositoryMock.Setup(repo => repo.GetTokenByTokenValue(requestToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeToken);

        // Act
        var result = await _sut.ResetPassword(requestToken, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthenticationErrors.InvalidArgs, result.FirstError);

        _tokenRepositoryMock.Verify(
            repo => repo.GetTokenByTokenValue(requestToken, It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }

    [Fact]
    public async Task ResetPassword_WhenRequestTokenIsAlreadyUsed_ShouldReturnInvalidArgsError()
    {
        // Arrange
        string requestToken = "some-generated-token";
        ResetPassRequestDto request = new ResetPassRequestDto
        {
            Password = "Password123!"
        };

        Token existingToken = new Token
        {
            TokenValue = "some-generated-token",
            IsUsed = true
        };

        _tokenRepositoryMock.Setup(repo => repo.GetTokenByTokenValue(requestToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        // Act
        var result = await _sut.ResetPassword(requestToken, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthenticationErrors.InvalidArgs, result.FirstError);

        _tokenRepositoryMock.Verify(
            repo => repo.GetTokenByTokenValue(requestToken, It.IsAny<CancellationToken>()),
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
        existingUser.Role = new UserRole
        {
            Id = existingUser.RoleId,
            UserRoleName = UserRoleEnum.RegularUser.ToString(),
            RolePermissions = new List<RolePermission>
            {
                new RolePermission
                {
                    RoleId = existingUser.RoleId,
                    PermissionId = 1,
                    ExternalId = Guid.NewGuid(),
                    Permission = new Permission
                    {
                        Id = 1,
                        PermissionName = PermissionNames.UserRead
                    }
                },

                new RolePermission
                {
                    RoleId = existingUser.RoleId,
                    PermissionId = 2,
                    ExternalId = Guid.NewGuid(),
                    Permission = new Permission
                    {
                        Id = 2,
                        PermissionName = PermissionNames.UserWrite
                    }
                }
            }
        };
        existingUser.PasswordHistory = new PasswordHistory
        {
            PasswordHash = "hashedPassword"
        };

        _tokenRepositoryMock.Setup(repo => repo.GetTokenByTokenValue(requestToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _passwordHasherMock.Setup(hasher => hasher.Hash(It.IsAny<string>()))
            .Returns(hashedPassword);

        _userRepositoryMock.Setup(repo => repo.GetUserById(existingToken.TokenUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _passwordHistoryRepositoryMock.Setup(repo => repo.GetByPasswordHash(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser.PasswordHistory);

        // Act
        var result = await _sut.ResetPassword(requestToken, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthenticationErrors.InvalidPassword, result.FirstError);

        _tokenRepositoryMock.Verify(
            repo => repo.GetTokenByTokenValue(requestToken, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(existingToken.TokenUserId, It.IsAny<CancellationToken>()), 
            Times.Once
        );

        _passwordHistoryRepositoryMock.Verify(
            repo => repo.GetByPasswordHash(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ResetPassword_WhenRequestIsValid_ShouldReturnTrue()
    {
        // Arrange
        string requestToken = "some-generated-token";
        string hashedPassword = "newHashedPassword";
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
        existingUser.Role = new UserRole
        {
            Id = existingUser.RoleId,
            UserRoleName = UserRoleEnum.RegularUser.ToString(),
            RolePermissions = new List<RolePermission>
            {
                new RolePermission
                {
                    RoleId = existingUser.RoleId,
                    PermissionId = 1,
                    ExternalId = Guid.NewGuid(),
                    Permission = new Permission
                    {
                        Id = 1,
                        PermissionName = PermissionNames.UserRead
                    }
                },

                new RolePermission
                {
                    RoleId = existingUser.RoleId,
                    PermissionId = 2,
                    ExternalId = Guid.NewGuid(),
                    Permission = new Permission
                    {
                        Id = 2,
                        PermissionName = PermissionNames.UserWrite
                    }
                }
            }
        };
        existingUser.PasswordHistory = new PasswordHistory
        {
            PasswordHash = "hashedPassword"
        };

        _tokenRepositoryMock.Setup(repo => repo.GetTokenByTokenValue(requestToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _passwordHasherMock.Setup(hasher => hasher.Hash(It.IsAny<string>()))
            .Returns(hashedPassword);

        _userRepositoryMock.Setup(repo => repo.GetUserById(existingToken.TokenUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _passwordHistoryRepositoryMock.Setup(repo => repo.GetByPasswordHash(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordHistory?)null);

        _passwordHistoryRepositoryMock.Setup(repo => repo.Add(It.IsAny<PasswordHistory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<int>());

        _userRepositoryMock.Setup(repo => repo.ApplyBehaviorChanges(It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<bool>());

        _tokenRepositoryMock.Setup(repo => repo.ApplyBehaviorChanges(It.IsAny<CancellationToken>()))
           .ReturnsAsync(It.IsAny<bool>());

        // Act
        var result = await _sut.ResetPassword(requestToken, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(typeof(bool), result.Value.GetType());

        _tokenRepositoryMock.Verify(
            repo => repo.GetTokenByTokenValue(requestToken, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(existingToken.TokenUserId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _passwordHistoryRepositoryMock.Verify(
            repo => repo.GetByPasswordHash(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _passwordHistoryRepositoryMock.Verify(
            repo => repo.Add(It.IsAny<PasswordHistory>(), It.IsAny<CancellationToken>()),
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
