using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Records.Contracts.Requests;
using ExpenseTracker.Application.Records.Errors;
using ExpenseTracker.Application.Records.Services;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Domain.Categories.Repository;
using ExpenseTracker.Domain.Collection.Repository;
using ExpenseTracker.Domain.Records.Entity;
using ExpenseTracker.Domain.Records.Repository;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace ExpenseTrackerApplication.Tests.Records;

public class UpdateTransactionRecordUseCaseTests
{
    private readonly Mock<ITransactionRecordRepository> _transactionRecordRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITransactionRecordCategoryRepository> _transactionRecordCategoryRepositoryMock;
    private readonly Mock<ITransactionCollectionRepository> _transactionCollectionRepositoryMock;
    private readonly Mock<IValidator<AddTransactionRecordRequestDto>> _addRecordValidatorMock;
    private readonly Mock<IValidator<UpdateTransactionRecordRequestDto>> _updateRecordValidatorMock;
    private readonly Mock<IValidator<List<UpdateTransactionRecordRequestDto>>> _updateRecordsValidatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly TransactionRecordService _sut;

    public UpdateTransactionRecordUseCaseTests()
    {
        _transactionRecordRepositoryMock = new Mock<ITransactionRecordRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _transactionRecordCategoryRepositoryMock = new Mock<ITransactionRecordCategoryRepository>();
        _transactionCollectionRepositoryMock = new Mock<ITransactionCollectionRepository>();
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
            _addRecordValidatorMock.Object,
            _updateRecordValidatorMock.Object,
            _updateRecordsValidatorMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task UpdateTransactionRecord_WhenRecordDoesNotExist_ShoudReturnInvalidArgsError()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();

        UpdateTransactionRecordRequestDto request = new UpdateTransactionRecordRequestDto
        {
            TransactionCategoryExternalId = Guid.NewGuid().ToString(),
            TransactionExternalId = Guid.NewGuid().ToString(),
            TransactionValue = 5
        };

        User existingUser = new User
        {
            Id = 1,
            ExternalId = Guid.NewGuid()
        };

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _transactionRecordRepositoryMock.Setup(
            repo => repo.GetUserTransactionByCategoryExternalId(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>())
        ).ReturnsAsync((TransactionRecord?)null);

        // Act
        var result = await _sut.UpdateUserTransaction(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransactionRecordErrors.InvalidArgs);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId, 
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.GetUserTransactionByCategoryExternalId(
                Guid.Parse(request.TransactionExternalId),
                Guid.Parse(request.TransactionCategoryExternalId),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateTransactionRecord_WhenUserIsNotOwner_ShoudReturnNotOwnerError()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();

        UpdateTransactionRecordRequestDto request = new UpdateTransactionRecordRequestDto
        {
            TransactionCategoryExternalId = Guid.NewGuid().ToString(),
            TransactionExternalId = Guid.NewGuid().ToString(),
            TransactionValue = 5
        };

        User existingUser = new User
        {
            Id = 1,
            ExternalId = Guid.NewGuid()
        };

        TransactionRecord existingRecord = new TransactionRecord
        {
            Id = 3,
            ExternalId = Guid.Parse(request.TransactionExternalId),
            TransactionValue = 20,
            TransactionUserId = 2,
            TransactionCategory = new TransactionRecordCategory
            {
                Id = 1,
                ExternalId = Guid.Parse(request.TransactionCategoryExternalId)
            }
        };

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _transactionRecordRepositoryMock.Setup(
            repo => repo.GetUserTransactionByCategoryExternalId(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(existingRecord);

        // Act
        var result = await _sut.UpdateUserTransaction(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransactionRecordErrors.NotOwner);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId, 
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.GetUserTransactionByCategoryExternalId(
                Guid.Parse(request.TransactionExternalId),
                Guid.Parse(request.TransactionCategoryExternalId),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateTransactionRecord_WhenRequestIsValid_ShoudReturnAffectedRows()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();

        UpdateTransactionRecordRequestDto request = new UpdateTransactionRecordRequestDto
        {
            TransactionCategoryExternalId = Guid.NewGuid().ToString(),
            TransactionExternalId = Guid.NewGuid().ToString(),
            TransactionValue = 5
        };

        User existingUser = new User
        {
            Id = 1,
            ExternalId = Guid.NewGuid()
        };

        TransactionRecord existingRecord = new TransactionRecord
        {
            Id = 3,
            ExternalId = Guid.Parse(request.TransactionExternalId),
            TransactionValue = 20,
            TransactionUserId = existingUser.Id,
            TransactionCategoryId = 1
        };

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _transactionRecordRepositoryMock.Setup(
            repo => repo.GetUserTransactionByCategoryExternalId(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(existingRecord);

        _transactionRecordRepositoryMock.Setup(
            repo => repo.SaveChanges(
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateUserTransaction(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(1);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId, 
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.GetUserTransactionByCategoryExternalId(
                Guid.Parse(request.TransactionExternalId),
                Guid.Parse(request.TransactionCategoryExternalId),
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.SaveChanges(
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
