using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Errors;
using ExpenseTracker.Application.Authentication.AuthenticationServices;
using ExpenseTracker.Application.Authentication.Contracts.Request;
using ExpenseTracker.Application.Authentication.Contracts.Response;
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

namespace ExpenseTrackerApplication.Tests.Authentication;

public class LoginUseCaseTests
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

    public LoginUseCaseTests()
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
    public async Task Login_WhenUserDoesntExist_ShouldReturnInvalidArgs()
    {
        // Arrange
        LoginRequestDto request = new LoginRequestDto 
        { 
            Email = "john@doe.com", 
            Password = "Password123!" 
        };

        string hashedPassword = "HashedPassword";

        _userRepositoryMock.Setup(repo => repo.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock.Setup(hasher => hasher.Hash(It.IsAny<string>()))
            .Returns(hashedPassword);

        // Act
        var result = await _sut.Login(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthenticationErrors.InvalidArgs, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }

    [Fact]
    public async Task Login_WhenPasswordDoesNotMatch_ShouldReturnInvalidArgs()
    {
        // Arrange
        LoginRequestDto request = new LoginRequestDto
        {
            Email = "john@doe.com",
            Password = "Password123!"
        };

        User existingUser = new User
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser
        };

        _userRepositoryMock.Setup(repo => repo.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _passwordHasherMock.Setup(hasher => hasher.Hash(It.IsAny<string>()))
            .Returns(request.Password);

        // Act
        var result = await _sut.Login(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthenticationErrors.InvalidArgs, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Login_WhenRequestIsValid_ShouldReturnLoginResponseDto()
    {
        // Arrange
        LoginRequestDto request = new LoginRequestDto
        {
            Email = "john@doe.com",
            Password = "Password123!"
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

        _userRepositoryMock.Setup(repo => repo.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _passwordHasherMock.Setup(hasher => hasher.Hash(request.Password))
            .Returns(existingUser.Password);

        _tokenGeneratorMock.Setup(
            service => service.GenerateAccessToken(
                existingUser.ExternalId,
                It.IsAny<List<string>>(),
                 It.IsAny<string>(), 
                It.IsAny<CancellationToken>())
        ).Returns("someAccessToken");

        _tokenGeneratorMock.Setup(
            service => service.GenerateRefreshToken(
                existingUser.ExternalId,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())
        ).Returns("someRefreshToken");

        _tokenServiceMock.Setup(service => service.AddToken(
            It.IsAny<Token>(),
            It.IsAny<CancellationToken>())
        ).ReturnsAsync(new Token());

        // Act
        var result = await _sut.Login(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal("someAccessToken", result.Value.AccessToken);
        Assert.Equal("someRefreshToken", result.Value.RefreshToken);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _tokenServiceMock.Verify(
            service => service.AddToken(
                It.IsAny<Token>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}
