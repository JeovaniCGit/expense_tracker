using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Errors;
using ExpenseTracker.Application.Authentication.AuthenticationServices;
using ExpenseTracker.Application.Authentication.Contracts.Request;
using ExpenseTracker.Application.Authentication.Errors;
using ExpenseTracker.Application.Authentication.JwtLib;
using ExpenseTracker.Application.Authorization.BCryptLib;
using ExpenseTracker.Application.Authorization.Tokens.Enums;
using ExpenseTracker.Application.Authorization.Tokens.Services;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Authorization.Tokens.Entity;
using ExpenseTracker.Domain.Authorization.Tokens.Repository;
using ExpenseTracker.Domain.Email.Repository;
using FluentValidation;
using Hangfire;
using Moq;

namespace ExpenseTrackerApplication.Tests.Authentication;

public class RegisterUseCaseTests
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

    public RegisterUseCaseTests()
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
    public async Task Register_WhenUserAlreadyExists_ShouldReturnDuplicatedEntryError()
    {
        // Arrange
        var request = new AddUserRequestDto
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "Password123!"
        };

        User existingUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Foe",
            Email = "john@doe.com",
            Password = "Password1234!"
        };

        _userRepositoryMock.Setup(repo => repo.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _sut.Register(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthenticationErrors.DuplicatedEntry, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }

    [Fact]
    public async Task Register_WhenRequestIsValid_ShouldReturnAddUserResponseDto()
    {
        // Arrange
        var fixedTimestamp = new DateTimeOffset(2024, 3, 15, 10, 30, 0, TimeSpan.Zero);

        var request = new AddUserRequestDto
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "Password123!"
        };

        User addedUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "Password123!",
            ExternalId = Guid.NewGuid(),
            CreatedAt = fixedTimestamp
        };

        _userRepositoryMock.Setup(repo => repo.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock.Setup(repo => repo.CreateUser(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(addedUser);

        _tokenServiceMock.Setup(service => service.GenerateToken(It.IsAny<TokenDescriptionEnum>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(new Token
            {
                TokenValue = "generatedtoken",
            });

        // Act
        var result = await _sut.Register(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(addedUser.ExternalId, result.Value.ExternalId);
        Assert.Equal(addedUser.Firstname, result.Value.Firstname);
        Assert.Equal(addedUser.Lastname, result.Value.Lastname);
        Assert.Equal(fixedTimestamp, result.Value.CreatedAt);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByEmail(request.Email, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.CreateUser(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
