using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Authentication.AuthenticationServices;
using ExpenseTracker.Application.Authentication.Contracts.Request;
using ExpenseTracker.Application.Authentication.Errors;
using ExpenseTracker.Application.Authentication.JwtLib;
using ExpenseTracker.Application.Authorization.BCryptLib;
using ExpenseTracker.Application.Authorization.Tokens.Enums;
using ExpenseTracker.Application.Authorization.Tokens.Services;
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

public class ForgotPasswordUseCaseTests
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

    public ForgotPasswordUseCaseTests()
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
    public async Task ForgotPassword_WhenUserEmailIsNullOrEmpty_ShouldReturnInvalidArgsError()
    {
        // Arrange
        string requestEmail = string.Empty;

        // Act
        var result = await _sut.ForgotPassword(requestEmail);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AuthenticationErrors.InvalidArgs);
    }

    [Fact]
    public async Task ForgotPassword_WhenUserDoesNotExist_ShouldReturnInvalidArgsError()
    {
        // Arrange
        string requestEmail = "john@doe.com";

        _userRepositoryMock.Setup(
            repo => repo.GetUserByEmail(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.ForgotPassword(requestEmail);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(AuthenticationErrors.InvalidArgs);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByEmail(
                requestEmail,
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ForgotPassword_WhenRequestIsValid_ShouldReturnTrue()
    {
        // Arrange
        string requestEmail = "john@doe.com";

        User? existingUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = requestEmail,
            Password = "hashedpassword"
        };

        Token? capturedToken = null;
        EmailDelivery? capturedEmail = null;

        Token createdToken = new Token
        {
            TokenValue = "some-generated-token",
            IsUsed = false,
            TokenUserId = 1
        };

        EmailDelivery emailStatus = new EmailDelivery
        {
            UserId = existingUser.Id,
            Status = EmailDeliveryStatus.Verification.ToString(),
            SentAt = null
        };

        _userRepositoryMock.Setup(
            repo => repo.GetUserByEmail(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _tokenServiceMock.Setup(
            service => service.GenerateToken(
                It.IsAny<TokenDescriptionEnum>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .Returns(createdToken);

        _tokenServiceMock.Setup(
            service => service.AddToken(
                It.IsAny<Token>(),
                It.IsAny<CancellationToken>()))
        .Callback<Token, CancellationToken>((token, _) => capturedToken = token)
        .ReturnsAsync(It.IsAny<Token>());

        _emailDeliveryRepositoryMock.Setup(
            repo => repo.Add(
                It.IsAny<EmailDelivery>()))
        .Callback<EmailDelivery>((email) => capturedEmail = email)
        .Returns(It.IsAny<int>());

        // Act
        var result = await _sut.ForgotPassword(requestEmail);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();

        capturedEmail?.UserId.Should().Be(existingUser.Id);
        capturedEmail?.Status.Should().Be(EmailDeliveryStatus.Verification.ToString());

        capturedToken?.TokenValue.Should().Be(createdToken.TokenValue);
        capturedToken?.TokenUserId.Should().Be(existingUser.Id);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByEmail(
                requestEmail,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _tokenServiceMock.Verify(
            service => service.AddToken(
                It.IsAny<Token>(),
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _emailDeliveryRepositoryMock.Verify(
            repo => repo.Add(It.IsAny<EmailDelivery>()),
            Times.Once
        );
    }
}
