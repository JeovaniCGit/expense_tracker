using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Application.Records.Contracts.Requests;
using ExpenseTracker.Application.Records.Errors;
using ExpenseTracker.Application.Records.Services;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Authorization.UserRoles.Entity;
using ExpenseTracker.Domain.Categories.Repository;
using ExpenseTracker.Domain.Collection.Repository;
using ExpenseTracker.Domain.Records.Entity;
using ExpenseTracker.Domain.Records.Repository;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ExpenseTrackerApplication.Tests.Records;

public class DeleteTransactionRecordUseCaseTests
{
    private readonly Mock<ITransactionRecordRepository> _transactionRecordRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITransactionRecordCategoryRepository> _transactionRecordCategoryRepositoryMock;
    private readonly Mock<ITransactionCollectionRepository> _transactionCollectionRepositoryMock;
    private readonly Mock<IHttpContextAccessor> _contextMock;
    private readonly Mock<IValidator<AddTransactionRecordRequestDto>> _addRecordValidatorMock;
    private readonly Mock<IValidator<UpdateTransactionRecordRequestDto>> _updateRecordValidatorMock;
    private readonly Mock<IValidator<List<UpdateTransactionRecordRequestDto>>> _updateRecordsValidatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly TransactionRecordService _sut;

    public DeleteTransactionRecordUseCaseTests()
    {
        _transactionRecordRepositoryMock = new Mock<ITransactionRecordRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _transactionRecordCategoryRepositoryMock = new Mock<ITransactionRecordCategoryRepository>();
        _transactionCollectionRepositoryMock = new Mock<ITransactionCollectionRepository>();
        _contextMock = new Mock<IHttpContextAccessor>();
        _addRecordValidatorMock = new Mock<IValidator<AddTransactionRecordRequestDto>>();
        _updateRecordValidatorMock = new Mock<IValidator<UpdateTransactionRecordRequestDto>>();
        _updateRecordsValidatorMock = new Mock<IValidator<List<UpdateTransactionRecordRequestDto>>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _sut = new TransactionRecordService
        (
            _transactionRecordRepositoryMock.Object,
            _userRepositoryMock.Object,
            _transactionRecordCategoryRepositoryMock.Object,
            _transactionCollectionRepositoryMock.Object,
            _contextMock.Object,
            _addRecordValidatorMock.Object,
            _updateRecordValidatorMock.Object,
            _updateRecordsValidatorMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task DeleteTransactionRecord_WhenRequestRecordDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        long requestUserId = 1;
        Guid requestTargetExternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = Guid.NewGuid()
        };

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(requestUserId);

        _userRepositoryMock.Setup(repo => repo.GetUserById(requestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _transactionRecordRepositoryMock.Setup(repo => repo.GetTransactionRecordByExternalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TransactionRecord?)null);

        // Act
        var result = await _sut.DeleteTransactionRecord(requestTargetExternalId.ToString(), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(TransactionRecordErrors.NotFound, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(requestUserId, It.IsAny<CancellationToken>()),
            Times.Once()
        );

        _transactionRecordRepositoryMock.Verify(
           repo => repo.GetTransactionRecordByExternalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
           Times.Once
       );
    }

    [Fact]
    public async Task DeleteTransactionRecord_WhenRequestUserIsNotOwner_ShouldReturnNotOwnerError()
    {
        // Arrange
        long requestUserId = 1;
        Guid requestTargetExternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = Guid.NewGuid()
        };
        existingUser.Role = new UserRole
        {
            Id = (long)UserRoleEnum.RegularUser,
            UserRoleName = UserRoleEnum.RegularUser.ToString()
        };

        TransactionRecord existingRecord = new TransactionRecord
        {
            TransactionValue = 5,
            ExternalId = Guid.NewGuid(),
            TransactionUserId = 2
        };

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(requestUserId);

        _userRepositoryMock.Setup(repo => repo.GetUserById(requestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _transactionRecordRepositoryMock.Setup(repo => repo.GetTransactionRecordByExternalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRecord);

        // Act
        var result = await _sut.DeleteTransactionRecord(requestTargetExternalId.ToString(), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(TransactionRecordErrors.NotOwner, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(requestUserId, It.IsAny<CancellationToken>()),
            Times.Once()
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.GetTransactionRecordByExternalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteTransactionRecord_WhenRequestIsValid_ShouldReturnAffectedRows()
    {
        // Arrange
        long requestUserId = 1;
        Guid requestTargetExternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
            Firstname = "John",
            Lastname = "Doe",
            Email = "john@doe.com",
            Password = "hashedpassword",
            RoleId = (long)UserRoleEnum.RegularUser,
            ExternalId = Guid.NewGuid()
        };
        existingUser.Role = new UserRole
        {
            Id = (long)UserRoleEnum.RegularUser,
            UserRoleName = UserRoleEnum.RegularUser.ToString()
        };

        TransactionRecord existingRecord = new TransactionRecord
        {
            TransactionValue = 5,
            ExternalId = requestTargetExternalId,
            TransactionUserId = existingUser.Id,
        };

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(requestUserId);

        _userRepositoryMock.Setup(repo => repo.GetUserById(requestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _transactionRecordRepositoryMock.Setup(repo => repo.GetTransactionRecordByExternalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRecord);

        // Act
        var result = await _sut.DeleteTransactionRecord(requestTargetExternalId.ToString(), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(typeof(int), result.Value.GetType());

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(requestUserId, It.IsAny<CancellationToken>()),
            Times.Once()
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.GetTransactionRecordByExternalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
