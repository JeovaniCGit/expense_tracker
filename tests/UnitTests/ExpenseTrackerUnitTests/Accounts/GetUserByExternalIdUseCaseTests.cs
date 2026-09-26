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

public class GetUserByExternalIdUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<UpdateUserRequestDto>> _updateUserValidatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UserService _sut;

    public GetUserByExternalIdUseCaseTests()
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
    public async Task GetUserByExternalId_WhenRequestIsValid_ShouldReturnGetUserResponseDto()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            ExternalId = currentUserExternalId
        };

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        // Act
        var result = await _sut.GetUserByExternalId(CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Firstname.Should().BeEquivalentTo(existingUser.Firstname);
        result.Value.Lastname.Should().BeEquivalentTo(existingUser.Lastname);
        result.Value.Email.Should().BeEquivalentTo(existingUser.Email);
        result.Value.UserExternalId.Should().Be(existingUser.ExternalId);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
