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

namespace ExpenseTracker.UnitTests.Accounts;

public class GetAllUsersUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<UpdateUserRequestDto>> _updateUserValidatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UserService _sut;

    public GetAllUsersUseCaseTests()
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
                ExternalId = Guid.NewGuid()
            },

            new User
            {
                Id = 2,
                Firstname = "Jane",
                Lastname = "Smith",
                Email = "jane@smith.com",
                ExternalId = Guid.NewGuid()
            }
        };

        _userRepositoryMock.Setup(repo => repo.GetAllUsers(1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(expectedUsers);

        // Act
        var result = await _sut.GetAllUsers(1, 10, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(u => expectedUsers.Any(item => item.ExternalId == u.UserExternalId));
        result.Should().OnlyContain(u => expectedUsers.Any(item => item.Firstname == u.Firstname));
        result.Should().OnlyContain(u => expectedUsers.Any(item => item.Lastname == u.Lastname));
        result.Should().OnlyContain(u => expectedUsers.Any(item => item.Email == u.Email));

        _userRepositoryMock.Verify(
            repo => repo.GetAllUsers(1, 10, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
