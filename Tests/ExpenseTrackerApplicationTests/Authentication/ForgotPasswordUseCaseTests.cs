using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
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
using ExpenseTracker.Domain.Email.Entity;
using ExpenseTracker.Domain.Email.Repository;
using FluentValidation;
using Hangfire;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTrackerApplication.Tests.Authentication;

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
    private readonly Mock<IValidator<AddUserRequestDto>> _addUserValidatorMock;
    private readonly Mock<IValidator<LoginRequestDto>> _loginValidatorMock;
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
    public async Task ForgotPassword_WhenUserEmailIsNullOrEmpty_ShouldReturnInvalidArgsError()
    {
        // Arrange
        string requestEmail = string.Empty;

        // Act
        var result = await _sut.ForgotPassword(requestEmail);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthenticationErrors.InvalidArgs, result.FirstError);
    }

    [Fact]
    public async Task ForgotPassword_WhenUserDoesNotExist_ShouldReturnInvalidArgsError()
    {
        // Arrange
        string requestEmail = "john@doe.com";

        _userRepositoryMock.Setup(repo => repo.GetUserByEmail(requestEmail))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.ForgotPassword(requestEmail);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthenticationErrors.InvalidArgs, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByEmail(requestEmail), 
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

        _userRepositoryMock.Setup(repo => repo.GetUserByEmail(requestEmail))
            .ReturnsAsync(existingUser);

        _tokenServiceMock.Setup(service => service.GenerateToken(It.IsAny<TokenDescriptionEnum>(), existingUser.Id, It.IsAny<CancellationToken>()))
            .Returns(new Token
            {
                TokenValue = "generatedToken"
            });

        _tokenServiceMock.Setup(service => service.AddToken(It.IsAny<Token>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<Token>());

        _emailDeliveryRepositoryMock.Setup(repo => repo.Add(It.IsAny<EmailDelivery>()))
            .Returns(It.IsAny<int>());

        // Act
        var result = await _sut.ForgotPassword(requestEmail);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(typeof(bool), result.Value.GetType());

        _userRepositoryMock.Verify(
            repo => repo.GetUserByEmail(requestEmail),
            Times.Once
        );

        _tokenServiceMock.Verify(
            service => service.AddToken(It.IsAny<Token>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _emailDeliveryRepositoryMock.Verify(
            repo => repo.Add(It.IsAny<EmailDelivery>()),
            Times.Once
        );
    }
}
