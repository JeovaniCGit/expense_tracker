using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authorization.BCryptLib;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace ExpenseTrackerApplication.Tests.Accounts;

public class CreateUserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHistoryRepository> _passwordHistoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IValidator<AddUserRequestDto>> _addUserValidatorMock;
    private readonly Mock<IValidator<UpdateUserRequestDto>> _updateUserValidatorMock;
    private readonly Mock<IDateProvider> _dateProviderMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UserService _sut;

    public CreateUserUseCaseTests()
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
    public async Task CreateUser_WhenRequestIsValid_ShouldReturnAddUserResponseDto()
    {
        // Arrange
        AddUserRequestDto request = new()
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "Password123!"
        };

        var fixedTimestamp = new DateTimeOffset(2024, 3, 15, 10, 30, 0, TimeSpan.Zero);

        User createdUser = new User
        {
            Firstname = request.Firstname,
            Lastname = request.Lastname,
            Email = request.Email,
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = Guid.NewGuid(),
            CreatedAt = fixedTimestamp
        };

        User? capturedUserData = null;

        _passwordHasherMock.Setup(
            hasher => hasher.Hash(
                It.IsAny<string>()))
        .Returns("hashedpassword");

        _userRepositoryMock.Setup(
            repo => repo.CreateUser(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
        .Callback<User, CancellationToken>((user, _) => capturedUserData = user)
        .ReturnsAsync(createdUser);

        // Act
        var result = await _sut.CreateUser(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Firstname.Should().BeEquivalentTo(createdUser.Firstname);
        result.Value.Lastname.Should().BeEquivalentTo(createdUser.Lastname);
        result.Value.ExternalId.Should().Be(createdUser.ExternalId);
        result.Value.CreatedAt.Should().Be(createdUser.CreatedAt);

        capturedUserData.Should().NotBeNull();
        capturedUserData.Firstname.Should().BeEquivalentTo(request.Firstname);
        capturedUserData.Lastname.Should().BeEquivalentTo(request.Lastname);
        capturedUserData.Email.Should().BeEquivalentTo(request.Email);
        capturedUserData.Password.Should().BeEquivalentTo("hashedpassword");
        capturedUserData.RoleId.Should().Be((long)UserRoleEnum.RegularUser);


        _userRepositoryMock.Verify(
            repo => repo.CreateUser(
                It.IsAny<User>(), 
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
