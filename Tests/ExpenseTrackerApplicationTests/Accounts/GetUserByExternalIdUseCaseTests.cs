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

public class GetUserByExternalIdUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHistoryRepository> _passwordHistoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IValidator<AddUserRequestDto>> _addUserValidatorMock;
    private readonly Mock<IValidator<UpdateUserRequestDto>> _updateUserValidatorMock;
    private readonly Mock<IDateProvider> _dateProviderMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UserService _sut;

    public GetUserByExternalIdUseCaseTests()
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
    public async Task GetUserByExternalId_WhenUserIsNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        Guid requestExternalId = Guid.NewGuid();
        _userRepositoryMock.Setup(repo => repo.GetUserByExternalId(requestExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.GetUserByExternalId(requestExternalId.ToString(), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(UserErrors.NotFound, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(requestExternalId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetUserByExternalId_WhenRequestIsValid_ShouldReturnGetUserResponseDto()
    {
        // Arrange
        Guid requestExternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = requestExternalId
        };

        _userRepositoryMock.Setup(repo => repo.GetUserByExternalId(requestExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _sut.GetUserByExternalId(requestExternalId.ToString(), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(existingUser.Firstname, result.Value.Firstname);
        Assert.Equal(existingUser.Lastname, result.Value.Lastname);
        Assert.Equal(existingUser.Email, result.Value.Email);
        Assert.Equal(existingUser.ExternalId, result.Value.UserExternalId);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(requestExternalId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
