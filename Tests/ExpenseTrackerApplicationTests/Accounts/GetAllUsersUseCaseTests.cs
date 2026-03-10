using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authorization.BCryptLib;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using FluentValidation;
using Moq;

namespace ExpenseTrackerApplication.Tests.Accounts;

public class GetAllUsersUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHistoryRepository> _passwordHistoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IValidator<AddUserRequestDto>> _addUserValidatorMock;
    private readonly Mock<IValidator<UpdateUserRequestDto>> _updateUserValidatorMock;
    private readonly Mock<IDateProvider> _dateProviderMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UserService _sut;

    public GetAllUsersUseCaseTests()
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
    public async Task GetAllUsers_WhenRequestIsValid_ShouldReturnEnumerableOfGetAllUsersResponseDto()
    {
        // Arrange
        IEnumerable<User> expectedUsers = new List<User>
        {
            new User
            {
                Id = 1,
                Firstname = "John",
                Lastname = "Doe",
                Email = "john@doe.com",
                Password = "hashedpassword",
                RoleId = (long)UserRoleEnum.RegularUser,
                ExternalId = Guid.NewGuid()
            },

            new User
            {
                Id = 2,
                Firstname = "Jane",
                Lastname = "Smith",
                Email = "jane@smith.com",
                Password = "hashedpassword",
                RoleId = (long)UserRoleEnum.RegularUser,
                ExternalId = Guid.NewGuid()
            }
        };

        _userRepositoryMock.Setup(repo => repo.GetAllUsers(1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(expectedUsers);

        // Act
        var result = await _sut.GetAllUsers(1, 10, CancellationToken.None);

        // Assert
        Assert.Collection(result,
            user =>
            {
                Assert.Equal("John", user.Firstname);
                Assert.Equal("Doe", user.Lastname);
                Assert.Equal("john@doe.com", user.Email);
            },
            user =>
            {
                Assert.Equal("Jane", user.Firstname);
                Assert.Equal("Smith", user.Lastname);
                Assert.Equal("jane@smith.com", user.Email);
            }
        );

        _userRepositoryMock.Verify(
            repo => repo.GetAllUsers(1, 10, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
