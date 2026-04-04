using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Collections.Errors;
using ExpenseTracker.Application.Records.Contracts.Requests;
using ExpenseTracker.Application.Records.Services;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Domain.Categories.Repository;
using ExpenseTracker.Domain.Collection.Repository;
using ExpenseTracker.Domain.Collections.Entity;
using ExpenseTracker.Domain.Records.Entity;
using ExpenseTracker.Domain.Records.Repository;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace ExpenseTrackerApplication.Tests.Records;

public class GetAllTransactionRecordsUseCaseTests
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

    public GetAllTransactionRecordsUseCaseTests()
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
    public async Task GetAllTransactionRecords_WhenCollectionDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        Guid colletionExternalId = Guid.NewGuid();
        Guid currentUserExternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
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

        _transactionCollectionRepositoryMock.Setup(
            repo => repo.GetUserCollectionByExternalId(
                It.IsAny<long>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync((TransactionCollection?)null);

        // Act
        var result = await _sut.GetAllTransactionsByCollectionId(colletionExternalId.ToString(), CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(CollectionErrors.NotFound);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionCollectionRepositoryMock.Verify(
            repo => repo.GetUserCollectionByExternalId(
                existingUser.Id,
                colletionExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAllTransactionRecords_WhenRequestIsValid_ShouldReturnIEnumerableOfGetTransactionRecordResponseDto()
    {
        // Arrange
        Guid colletionExternalId = Guid.NewGuid();
        Guid categoryExternalId = Guid.NewGuid();

        Guid currentUserExternalId = Guid.NewGuid();

        Guid record1ExternalId = Guid.NewGuid();
        Guid record2ExternalId = Guid.NewGuid();
        Guid record3ExternalId = Guid.NewGuid();


        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserExternalId
        };

        TransactionCollection existingCollection = new TransactionCollection
        {
            Id = 1,
            ExternalId = colletionExternalId,
            UserId = existingUser.Id
        };

        TransactionRecordCategory existingCategory = new TransactionRecordCategory
        {
            Id = 1,
            ExternalId = categoryExternalId,
            CategoryName = "Health"
        };

        IEnumerable<TransactionRecord> existingRecords = new List<TransactionRecord>()
        {
            new TransactionRecord
            {
                Id = 1,
                ExternalId = record1ExternalId,
                TransactionValue = 5,
                TransactionUserId = existingUser.Id,
                TransactionCategory = existingCategory
            },
            new TransactionRecord
            {
                Id = 2,
                ExternalId = record2ExternalId,
                TransactionValue = 10,
                TransactionUserId = existingUser.Id,
                TransactionCategory = existingCategory
            },
            new TransactionRecord
            {
                Id = 3,
                ExternalId = record3ExternalId,
                TransactionValue = 20,
                TransactionUserId = existingUser.Id,
                TransactionCategory = existingCategory
            },
        };

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _transactionCollectionRepositoryMock.Setup(
            repo => repo.GetUserCollectionByExternalId(
                It.IsAny<long>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingCollection);

        _transactionRecordRepositoryMock.Setup(
            repo => repo.GetAllUserTransactionsByCollection(
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingRecords);

        // Act
        var result = await _sut.GetAllTransactionsByCollectionId(colletionExternalId.ToString(), CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(3);
        result.Value.Should().OnlyContain(r => existingRecords.Any(item => item.ExternalId == r.TransactionExternalId));
        result.Value.Should().OnlyContain(r => existingRecords.Any(item => item.TransactionValue == r.TransactionValue));
        result.Value.Should().OnlyContain(r => existingCategory.ExternalId == r.TransactionCategoryExternalId);
        result.Value.Should().OnlyContain(r => existingCategory.CategoryName == r.TransactionCategoryName);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionCollectionRepositoryMock.Verify(
            repo => repo.GetUserCollectionByExternalId(
                existingUser.Id,
                colletionExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.GetAllUserTransactionsByCollection(
                existingUser.Id,
                existingCollection.Id,
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
