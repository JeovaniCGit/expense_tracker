using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Authentication.AuthenticationServices;
using ExpenseTracker.Application.Authentication.Contracts.Request;
using ExpenseTracker.Application.Authentication.Errors;
using ExpenseTracker.Application.Authentication.JwtLib;
using ExpenseTracker.Application.Authorization.BCryptLib;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Authorization.Tokens.Enums;
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
using System.Security.Claims;

namespace ExpenseTrackerApplication.Tests.Authentication;

public class RefreshTokenUseCaseTests
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

    public RefreshTokenUseCaseTests()
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
    public async Task RefreshToken_WhenTokenTypIsNotRefreshToken_ShouldReturnUnauthorizedError()
    {
        // Arrange
        RefreshRequestDto request = new RefreshRequestDto
        {
            RefreshToken = "some-refresh-token"
        };

        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("typ", "access_token")
        }));

        _tokenValidatorMock.Setup(service => service.Validate(request.RefreshToken))
            .Returns(claimsPrincipal);

        // Act
        var result = await _sut.RefreshToken(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthenticationErrors.Unauthorized, result.FirstError);
    }

    [Fact]
    public async Task RefreshToken_WhenUserDoesNotExist_ShouldReturnUnauthorizedError()
    {
        // Arrange
        RefreshRequestDto request = new RefreshRequestDto
        {
            RefreshToken = "some-refresh-token"
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

        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("typ", TokenDescriptionEnum.RefreshToken.ToString()),
            new Claim("sub", existingUser.ExternalId.ToString())
        }));

        _tokenValidatorMock.Setup(service => service.Validate(request.RefreshToken))
            .Returns(claimsPrincipal);

        _userRepositoryMock.Setup(repo => repo.GetUserByExternalId(existingUser.ExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.RefreshToken(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthenticationErrors.Unauthorized, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(existingUser.ExternalId, It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshToken_WhenTokenDoesNotExistOrIsInvalid_ShouldReturnInvalidArgsError()
    {
        // Arrange
        RefreshRequestDto request = new RefreshRequestDto
        {
            RefreshToken = "some-refresh-token"
        };

        Token fakeToken = new Token
        {
            TokenValue = "some-fake-refresh-token"
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

        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("typ", TokenDescriptionEnum.RefreshToken.ToString()),
            new Claim("sub", existingUser.ExternalId.ToString())
        }));

        _tokenValidatorMock.Setup(service => service.Validate(request.RefreshToken))
            .Returns(claimsPrincipal);

        _userRepositoryMock.Setup(repo => repo.GetUserByExternalId(existingUser.ExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _tokenGeneratorMock.Setup(service => service.GenerateAccessToken(
            It.IsAny<Guid>(), 
            It.IsAny<List<string>>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>())
        ).Returns("some-access-token");

        _tokenGeneratorMock.Setup(service => service.GenerateRefreshToken(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())
        ).Returns("some-refresh-token");

        _tokenRepositoryMock.Setup(repo => repo.GetTokenByTokenValue(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeToken);

        // Act
        var result = await _sut.RefreshToken(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthenticationErrors.InvalidArgs, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(existingUser.ExternalId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _tokenRepositoryMock.Verify(
            repo => repo.GetTokenByTokenValue(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshToken_WhenTokenWasAlreadyUsed_ShouldReturnInvalidArgsError()
    {
        // Arrange
        RefreshRequestDto request = new RefreshRequestDto
        {
            RefreshToken = "some-refresh-token"
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

        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("typ", TokenDescriptionEnum.RefreshToken.ToString()),
            new Claim("sub", existingUser.ExternalId.ToString())
        }));

        _tokenValidatorMock.Setup(service => service.Validate(request.RefreshToken))
            .Returns(claimsPrincipal);

        _userRepositoryMock.Setup(repo => repo.GetUserByExternalId(existingUser.ExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _tokenGeneratorMock.Setup(service => service.GenerateAccessToken(
            It.IsAny<Guid>(),
            It.IsAny<List<string>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())
        ).Returns("some-access-token");

        _tokenGeneratorMock.Setup(service => service.GenerateRefreshToken(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())
        ).Returns("some-refresh-token");

        _tokenRepositoryMock.Setup(repo => repo.GetTokenByTokenValue(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Token
            {
                IsUsed = true
            });

        // Act
        var result = await _sut.RefreshToken(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthenticationErrors.InvalidArgs, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(existingUser.ExternalId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _tokenRepositoryMock.Verify(
            repo => repo.GetTokenByTokenValue(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshToken_WhenRefreshIsValid_ShouldReturnRefreshTokenResponseDto()
    {
        // Arrange
        RefreshRequestDto request = new RefreshRequestDto
        {
            RefreshToken = "some-refresh-token"
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

        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("typ", TokenDescriptionEnum.RefreshToken.ToString()),
            new Claim("sub", existingUser.ExternalId.ToString())
        }));

        Token existingToken = new Token
        {
            TokenValue = "some-refresh-token",
            IsUsed = false
        };

        _tokenValidatorMock.Setup(service => service.Validate(request.RefreshToken))
            .Returns(claimsPrincipal);

        _userRepositoryMock.Setup(repo => repo.GetUserByExternalId(existingUser.ExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _tokenGeneratorMock.Setup(service => service.GenerateAccessToken(
            It.IsAny<Guid>(),
            It.IsAny<List<string>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())
        ).Returns("some-access-token");

        _tokenGeneratorMock.Setup(service => service.GenerateRefreshToken(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>())
        ).Returns("some-refresh-token");

        _tokenRepositoryMock.Setup(repo => repo.GetTokenByTokenValue(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _tokenRepositoryMock.Setup(repo => repo.AddToken(It.IsAny<Token>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<Token>());

        // Act
        var result = await _sut.RefreshToken(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal("some-access-token", result.Value.AccessToken);
        Assert.Equal("some-refresh-token", result.Value.RefreshToken);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(existingUser.ExternalId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _tokenRepositoryMock.Verify(
            repo => repo.GetTokenByTokenValue(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _tokenRepositoryMock.Verify(
            repo => repo.AddToken(It.IsAny<Token>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
