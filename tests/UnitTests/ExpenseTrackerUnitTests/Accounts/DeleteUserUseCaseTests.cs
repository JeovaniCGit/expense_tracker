using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Errors;
using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authorization.BCryptLib;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace ExpenseTracker.UnitTests.Accounts;

public class DeleteUserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<UpdateUserRequestDto>> _updateUserValidatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UserService _sut;

    public DeleteUserUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _updateUserValidatorMock = new Mock<IValidator<UpdateUserRequestDto>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _sut = new UserService(
            _userRepositoryMock.Object,
            _updateUserValidatorMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task DeleteUser_WhenTargetUserDoesNotMatch_ShouldReturnInvalidArgsError()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();
        Guid targetUserExternalId = Guid.NewGuid();
        
        User targetUser = new User
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            ExternalId = Guid.NewGuid()
        };

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(targetUser);

        // Act
        var result = await _sut.DeleteUser(null, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(UserErrors.InvalidArgs);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId, 
                It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteUser_WhenUserIsNotOwnerOfAccountOrAdmin_ShouldReturnInvalidArgsError()
    {
        // Arrange
        User currentUser = new User
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            ExternalId = Guid.NewGuid()
        };

        User targetUser = new User
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            ExternalId = Guid.NewGuid()
        };
        
        var mockRequestExternalId = targetUser.ExternalId;

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUser.ExternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(targetUser);

        // Act
        var result = await _sut.DeleteUser(targetUser.ExternalId.ToString(), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeEquivalentTo(UserErrors.InvalidArgs);

        _userRepositoryMock.Verify(
          repo => repo.GetUserByExternalId(
              mockRequestExternalId,
              It.IsAny<CancellationToken>()),
          Times.Once
      );
    }

    [Fact]
    public async Task DeleteUser_WhenRequestIsValid_ShouldReturnAffectedRows()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();
        Guid targetUserExternalId = currentUserExternalId;

        User currentUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            ExternalId = currentUserExternalId
        };

        User targetUser = new User
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            ExternalId = targetUserExternalId
        };

        User? capturedUserData = null;

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(targetUser);

        _userRepositoryMock.Setup(
            repo => repo.DeleteUser(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
        .Callback<User, CancellationToken>((user, _) => capturedUserData = user)
        .ReturnsAsync(1);

        // Act
        var result = await _sut.DeleteUser(null, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(1);

        capturedUserData.Should().NotBeNull();
        capturedUserData.Should().BeEquivalentTo(targetUser);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId, 
                It.IsAny<CancellationToken>()), 
            Times.AtMost(2)
        );

        _userRepositoryMock.Verify(
           repo => repo.DeleteUser(
               It.IsAny<User>(), 
               It.IsAny<CancellationToken>()),
           Times.Once
       );
    }
}