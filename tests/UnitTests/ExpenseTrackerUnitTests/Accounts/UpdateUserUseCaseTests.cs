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
        Guid currentUserExternalId = Guid.NewGuid();
        Guid targetUserExternalId = Guid.NewGuid();
        UpdateUserRequestDto request = new()
        {
            UserExternalId = targetUserExternalId.ToString(),
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
        };

        User currentUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = currentUserExternalId
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
        .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.UpdateUser(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.NotFound);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId,
                It.IsAny<CancellationToken>()), 
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                targetUserExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
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
            UserExternalId = targetUserExternalId.ToString(),
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
        };

        User currentUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = currentUserExternalId
        };

        User targetUser = new User
        {
            Id = 2,
            Firstname = "jane",
            Lastname = "Doe",
            Email = "jane@doe.com",
            Password = "hashedpassword123",
            RoleId = (long)UserRoleEnum.RegularUser,
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
        result.FirstError.Should().Be(UserErrors.Forbidden);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                targetUserExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateUser_WhenNewPasswordWasAlreadyUsed_ShouldReturnInvalidPasswordError()
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
            Password = "hashedpassword123!",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = currentUserExternalId
        };

        User targetUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword123!",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = currentUserExternalId
        };

        UpdateUserRequestDto request = new()
        {
            UserExternalId = targetUserExternalId.ToString(),
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "Password123!"
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


        _passwordHasherMock.Setup(
            hasher => hasher.Hash(request.Password))
        .Returns("hashedPassword");

        _passwordHistoryMock.Setup(
            repo => repo.GetByPasswordHash(
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(new PasswordHistory
        {
            UserId = currentUser.Id,
            PasswordHash = "hashedPassword"
        });

        // Act
        var result = await _sut.UpdateUser(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(UserErrors.InvalidPassword);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId, 
                It.IsAny<CancellationToken>()),
            Times.AtMost(2)
        );

        _passwordHistoryMock.Verify(
            repo => repo.GetByPasswordHash(
                "hashedPassword", 
                It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateUser_WhenRequestIsValid_ShouldReturnAffectedRows()
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
            Password = "hashedpassword123!",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = currentUserExternalId
        };

        User targetUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword123!",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = currentUserExternalId
        };

        UpdateUserRequestDto request = new()
        {
            UserExternalId = targetUserExternalId.ToString(),
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "Password123!"
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

        string requestPasswordHash = "hashedPassword123!";

        _passwordHasherMock.Setup(
            hasher => hasher.Hash(
                It.IsAny<string>()))
        .Returns(requestPasswordHash);

        _passwordHistoryMock.Setup(
            repo => repo.GetByPasswordHash(
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync((PasswordHistory?)null);

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
        capturedDataToUpdate.Password.Should().Be(targetUser.Password);
        capturedDataToUpdate.Email.Should().Be(targetUser.Email);


        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                Guid.Parse(request.UserExternalId), 
                It.IsAny<CancellationToken>()),
            Times.AtMost(2)
        );

        _passwordHistoryMock.Verify(
            repo => repo.GetByPasswordHash(
                requestPasswordHash, 
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.UpdateUser(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
