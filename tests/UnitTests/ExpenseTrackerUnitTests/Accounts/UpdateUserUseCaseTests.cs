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

public class UpdateUserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<UpdateUserRequestDto>> _updateUserValidatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UserService _sut;

    public UpdateUserUseCaseTests()
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
    public async Task UpdateUser_WhenUserIsNotOwner_ShouldReturnForbiddenError()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();
        Guid targetUserExternalId = Guid.NewGuid();
        UpdateUserRequestDto request = new()
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Version = 123
        };

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
            Id = 2,
            Firstname = "jane",
            Lastname = "Doe",
            Email = "jane@doe.com",
            ExternalId = targetUserExternalId
        };

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _userRepositoryMock.Setup(
           repo => repo.GetUserByExternalId(
               It.IsAny<Guid>(),
               It.IsAny<CancellationToken>()))
       .ReturnsAsync(currentUser);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(targetUser);

        // Act
        var result = await _sut.UpdateUser(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.InvalidArgs);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateUser_WhenRequestIsValid_ShouldReturnAffectedRows()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();
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
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            ExternalId = currentUserExternalId
        };

        UpdateUserRequestDto request = new()
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Version = 123
        };

        User? capturedDataToUpdate = null;

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _userRepositoryMock.Setup(
           repo => repo.GetUserByExternalId(
               It.IsAny<Guid>(),
               It.IsAny<CancellationToken>()))
       .ReturnsAsync(currentUser);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(targetUser);

        _userRepositoryMock.Setup(
            repo => repo.UpdateUser(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
        .Callback<User, CancellationToken>((userData, _) => capturedDataToUpdate = userData)
        .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateUser(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(1);

        capturedDataToUpdate.Should().NotBeNull();
        capturedDataToUpdate.Id.Should().Be(targetUser.Id);
        capturedDataToUpdate.Firstname.Should().Be(targetUser.Firstname);
        capturedDataToUpdate.Lastname.Should().Be(targetUser.Lastname);
        capturedDataToUpdate.Email.Should().Be(targetUser.Email);


        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()),
            Times.AtMost(2)
        );

        _userRepositoryMock.Verify(
            repo => repo.UpdateUser(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
