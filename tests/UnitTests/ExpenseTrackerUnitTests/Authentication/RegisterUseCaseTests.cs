using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Authentication.AuthenticationServices;
using ExpenseTracker.Application.Authentication.Contracts.Request;
using ExpenseTracker.Application.Authentication.Errors;
using ExpenseTracker.Application.Authentication.JwtLib;
using ExpenseTracker.Application.Authorization.BCryptLib;
using ExpenseTracker.Application.Authorization.Tokens.Enums;
using ExpenseTracker.Application.Authorization.Tokens.Services;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Application.Emails.Enums;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Authorization.Tokens.Entity;
using ExpenseTracker.Domain.Authorization.Tokens.Repository;
using ExpenseTracker.Domain.Email.Entity;
using ExpenseTracker.Domain.Email.Repository;
using FluentAssertions;
using FluentValidation;
using Hangfire;
using Moq;

namespace ExpenseTracker.UnitTests.Authentication;

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
    private readonly Mock<IVerificationTokenObserver> _tokenObserverMock;
    private readonly Mock<IValidator<AddUserRequestDto>> _addUserValidatorMock;
    private readonly Mock<IValidator<LoginRequestDto>> _loginValidatorMock;
    private readonly Mock<IValidator<ResetPassRequestDto>> _resetPasswordValidatorMock;
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

        _userRepositoryMock.Setup(
            repo => repo.GetUserByEmail(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        // Act
        var result = await _sut.Register(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(AuthenticationErrors.DuplicatedEntry);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByEmail(
                request.Email,
                It.IsAny<CancellationToken>()),
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

        EmailDelivery emailStatus = new EmailDelivery
        {
            UserId = addedUser.Id,
            Status = EmailDeliveryStatus.Verification.ToString(),
            SentAt = null
        };

        Token verificatonToken = new Token
        {
            TokenValue = "some-token",
            TokenTypeId = (long)TokenDescriptionEnum.EmailVerificationToken,
            TokenUserId = addedUser.Id
        };

        User? capturedUserData = null;
        EmailDelivery? capturedStatus = null;
        Token? capturedAddedToken = null;

        _userRepositoryMock.Setup(
            repo => repo.GetUserByEmail(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync((User?)null);

        _passwordHasherMock.Setup(
            hasher => hasher.Hash(
                It.IsAny<string>()))
        .Returns("hashedPassword");

        _userRepositoryMock.Setup(
            repo => repo.CreateUser(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
        .Callback<User, CancellationToken>((user, _) => capturedUserData = user)
        .ReturnsAsync(addedUser);

        _tokenServiceMock.Setup(
            service => service.GenerateToken(
                It.IsAny<TokenDescriptionEnum>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
        .Returns(verificatonToken);

        _tokenRepositoryMock.Setup(
           repo => repo.AddToken(
            It.IsAny<Token>(),
            It.IsAny<CancellationToken>()))
       .Callback<Token, CancellationToken>((token, _) => capturedAddedToken = token)
       .ReturnsAsync(It.Is<Token>(
           t => t.TokenUserId == addedUser.Id));

        _emailDeliveryRepositoryMock.Setup(
            repo => repo.Add(It.IsAny<EmailDelivery>()))
        .Callback<EmailDelivery>((status) => capturedStatus = status)
        .Returns(1);

        // Act
        var result = await _sut.Register(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        capturedUserData.Should().NotBeNull();
        capturedUserData.Firstname.Should().Be(request.Firstname);
        capturedUserData.Lastname.Should().Be(request.Lastname);
        capturedUserData.Email.Should().Be(request.Email);
        capturedUserData.Password.Should().Be("hashedPassword");
        capturedUserData.RoleId.Should().Be((long)UserRoleEnum.RegularUser);

        capturedStatus?.UserId.Should().Be(addedUser.Id);
        capturedStatus?.Status.Should().Be(EmailDeliveryStatus.Verification.ToString());
        capturedStatus?.SentAt.Should().BeNull();

        capturedAddedToken?.TokenValue.Should().Be(verificatonToken.TokenValue);
        capturedAddedToken?.TokenTypeId.Should().Be(verificatonToken.TokenTypeId);
        capturedAddedToken?.TokenUserId.Should().Be(addedUser.Id);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByEmail(
                request.Email,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.CreateUser(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _tokenRepositoryMock.Verify(
            repo => repo.AddToken(
                It.IsAny<Token>(),
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _emailDeliveryRepositoryMock.Verify(
            repo => repo.Add(
                It.IsAny<EmailDelivery>()),
            Times.Once
        );
    }
}
