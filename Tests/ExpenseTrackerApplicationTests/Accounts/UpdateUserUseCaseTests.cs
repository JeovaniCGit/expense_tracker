using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Errors;
using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authorization.BCryptLib;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using FluentValidation;
using Moq;

namespace ExpenseTrackerApplication.Tests.Accounts;

public class UpdateUserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHistoryRepository> _passwordHistoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IValidator<AddUserRequestDto>> _addUserValidatorMock;
    private readonly Mock<IValidator<UpdateUserRequestDto>> _updateUserValidatorMock;
    private readonly Mock<IDateProvider> _dateProviderMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UserService _sut;

    public UpdateUserUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHistoryMock = new Mock<IPasswordHistoryRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _addUserValidatorMock = new Mock<IValidator<AddUserRequestDto>>();
        _updateUserValidatorMock = new Mock<IValidator<UpdateUserRequestDto>>();
        _dateProviderMock = new Mock<IDateProvider>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _sut = new UserService(
            _userRepositoryMock.Object,
            _passwordHistoryMock.Object,
            _passwordHasherMock.Object,
            _addUserValidatorMock.Object,
            _updateUserValidatorMock.Object,
            _dateProviderMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task UpdateUser_WhenUserDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        Guid externalId = Guid.NewGuid();
        UpdateUserRequestDto request = new()
        {
            UserExternalId = externalId.ToString(),
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
        };

        _userRepositoryMock.Setup(repo => repo.GetUserByExternalId(externalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.UpdateUser(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(UserErrors.NotFound, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(externalId, It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateUser_WhenNewPasswordWasAlreadyUsed_ShouldReturnInvalidPasswordError()
    {
        // Arrange
        Guid externalId = Guid.NewGuid();
        User existingUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser
        };

        UpdateUserRequestDto request = new()
        {
            UserExternalId = externalId.ToString(),
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "Password123!"
        };
        
        _userRepositoryMock.Setup(repo => repo.GetUserByExternalId(externalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _passwordHasherMock.Setup(hasher => hasher.Hash(request.Password)).Returns("hashedPassword");

        _passwordHistoryMock.Setup(repo => repo.GetByPasswordHash("hashedPassword", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PasswordHistory
            {
                UserId = existingUser.Id,
                PasswordHash = "hashedPassword"
            });

        // Act
        var result = await _sut.UpdateUser(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(UserErrors.InvalidPassword, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(externalId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _passwordHistoryMock.Verify(
            repo => repo.GetByPasswordHash("hashedPassword", It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateUser_WhenRequestIsValid_ShouldReturnAffectedRowsAsInt()
    {
        // Arrange
        User existingUser = new User
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = Guid.NewGuid()
        };

        UpdateUserRequestDto request = new()
        {
            UserExternalId = Guid.NewGuid().ToString(),
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "Password123!"
        };
        string requestPasswordHash = "hashedPassword123!";

        _userRepositoryMock.Setup(repo => repo.GetUserByExternalId(Guid.Parse(request.UserExternalId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _passwordHasherMock.Setup(hasher => hasher.Hash(It.IsAny<string>())).Returns(requestPasswordHash);

        _passwordHistoryMock.Setup(repo => repo.GetByPasswordHash(requestPasswordHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordHistory?)null);

        // Act
        var result = await _sut.UpdateUser(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(typeof(int), result.Value.GetType());

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(Guid.Parse(request.UserExternalId), It.IsAny<CancellationToken>()),
            Times.Once
        );

        _passwordHistoryMock.Verify(
            repo => repo.GetByPasswordHash(requestPasswordHash, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
