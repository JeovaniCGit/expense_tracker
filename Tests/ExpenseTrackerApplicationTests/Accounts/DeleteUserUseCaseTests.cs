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

public class DeleteUserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHistoryRepository> _passwordHistoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IValidator<AddUserRequestDto>> _addUserValidatorMock;
    private readonly Mock<IValidator<UpdateUserRequestDto>> _updateUserValidatorMock;
    private readonly Mock<IDateProvider> _dateProviderMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UserService _sut;

    public DeleteUserUseCaseTests()
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
    public async Task DeleteUser_WhenRequestUserDoesNotExist_ShouldReturnUnauthorizedError()
    {
        // Arrange
        Guid externalId = Guid.NewGuid();
        long requestUserId = 1;

        _currentUserServiceMock.Setup(service => service.UserId).Returns(requestUserId);

        _userRepositoryMock.Setup(repo => repo.GetUserById(requestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.DeleteUser(externalId.ToString(), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(UserErrors.Unauthorized, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(requestUserId, It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteUser_WhenTargetUserDoesNotExist_ShouldReturnInvalidArgsError()
    {
        // Arrange
        Guid requestUserExternalId = Guid.NewGuid();
        Guid targetUserExternalId = requestUserExternalId;

        User requestUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = requestUserExternalId
        };

        User targetUser = new User
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = targetUserExternalId
        };

        _currentUserServiceMock.Setup(service => service.UserId).Returns(requestUser.Id);

        _userRepositoryMock
            .Setup(repo => repo.GetUserById(requestUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestUser);

        _userRepositoryMock
            .Setup(repo => repo.GetUserByExternalId(targetUserExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.DeleteUser(targetUser.ExternalId.ToString(), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(UserErrors.InvalidArgs, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(requestUser.Id, It.IsAny<CancellationToken>()), 
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(targetUserExternalId, It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteUser_WhenUserIsNotOwnerOfAccountOrAdmin_ShouldReturnForbiddenError()
    {
        // Arrange
        Guid requestUserExternalId = Guid.NewGuid();
        Guid targetUserExternalId = Guid.NewGuid();

        User requestUser = new User
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = requestUserExternalId
        };

        User targetUser = new User
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = targetUserExternalId
        };

        _currentUserServiceMock.Setup(service => service.UserId).Returns(requestUser.Id);

        _userRepositoryMock
            .Setup(repo => repo.GetUserById(requestUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestUser);

        _userRepositoryMock
            .Setup(repo => repo.GetUserByExternalId(targetUserExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetUser);

        // Act
        var result = await _sut.DeleteUser(targetUser.ExternalId.ToString(), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(UserErrors.Forbidden, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(requestUser.Id, It.IsAny<CancellationToken>()), 
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(targetUserExternalId, It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteUser_WhenRequestIsValid_ShouldReturnAffectedRowsAsInt()
    {
        // Arrange
        Guid requestUserExternalId = Guid.NewGuid();
        Guid targetUserExternalId = requestUserExternalId;

        User requestUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = requestUserExternalId
        };

        User targetUser = new User
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = targetUserExternalId
        };

        _currentUserServiceMock.Setup(service => service.UserId).Returns(requestUser.Id);

        _userRepositoryMock
            .Setup(repo => repo.GetUserById(requestUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestUser);

        _userRepositoryMock
            .Setup(repo => repo.GetUserByExternalId(targetUserExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetUser);

        // Act
        var result = await _sut.DeleteUser(targetUser.ExternalId.ToString(), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(typeof(int), result.Value.GetType());

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(requestUser.Id, It.IsAny<CancellationToken>()), 
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(targetUserExternalId, It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }
}