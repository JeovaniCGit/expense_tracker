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
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ExpenseTrackerApplication.Tests.Records;

public class GetAllTransactionRecordsUseCaseTests
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

    public GetAllTransactionRecordsUseCaseTests()
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
    public async Task GetAllTransactionRecords_WhenCollectionDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        Guid colletionExternalId = Guid.NewGuid();
        User requestExistingUser = new User
        {
            Id = 1,
            ExternalId = Guid.NewGuid()
        };

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(requestExistingUser.Id);

        _userRepositoryMock.Setup(repo => repo.GetUserById(requestExistingUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestExistingUser);

        _transactionCollectionRepositoryMock.Setup(repo => repo.GetUserCollectionByExternalId(requestExistingUser.Id, colletionExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TransactionCollection?)null);

        // Act
        var result = await _sut.GetAllTransactionsByCollectionId(colletionExternalId.ToString(), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(CollectionErrors.NotFound, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(requestExistingUser.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionCollectionRepositoryMock.Verify(
            repo => repo.GetUserCollectionByExternalId(requestExistingUser.Id, colletionExternalId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAllTransactionRecords_WhenRequestIsValid_ShouldReturnIEnumerableOfGetTransactionRecordResponseDto()
    {
        // Arrange
        User requestExistingUser = new User
        {
            Id = 1,
            ExternalId = Guid.NewGuid(),
        };

        TransactionCollection existingCollection = new TransactionCollection
        {
            Id = 1,
            ExternalId = Guid.NewGuid(),
            UserId = requestExistingUser.Id,
        };

        TransactionRecordCategory existingCategory = new TransactionRecordCategory
        {
            Id = 1,
            ExternalId = Guid.NewGuid(),
            CategoryName = "Health"
        };

        IEnumerable<TransactionRecord> existingRecords = new List<TransactionRecord>()
        {
            new TransactionRecord
            {
                Id = 1,
                ExternalId= Guid.NewGuid(),
                TransactionValue = 5,
                TransactionUserId = requestExistingUser.Id,
                TransactionCategory = existingCategory
            },
            new TransactionRecord
            {
                Id = 2,
                ExternalId= Guid.NewGuid(),
                TransactionValue = 10,
                TransactionUserId = requestExistingUser.Id,
                TransactionCategory = existingCategory
            },
            new TransactionRecord
            {
                Id = 3,
                ExternalId= Guid.NewGuid(),
                TransactionValue = 20,
                TransactionUserId = requestExistingUser.Id,
                TransactionCategory = existingCategory
            },
        };


        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(requestExistingUser.Id);

        _userRepositoryMock.Setup(repo => repo.GetUserById(requestExistingUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestExistingUser);

        _transactionCollectionRepositoryMock.Setup(repo => repo.GetUserCollectionByExternalId(requestExistingUser.Id, existingCollection.ExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCollection);

        _transactionRecordRepositoryMock.Setup(repo => repo.GetAllUserTransactionsByCollection(requestExistingUser.Id, existingCollection.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRecords);

        // Act
        var result = await _sut.GetAllTransactionsByCollectionId(existingCollection.ExternalId.ToString(), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Collection(result.Value,
            record =>
            {
                Assert.Equal(5, record.TransactionValue);
                Assert.NotEqual(Guid.Empty, record.TransactionExternalId);
                Assert.Equal(existingCategory.ExternalId, record.TransactionCategoryExternalId);
                Assert.Equal(existingCategory.CategoryName, record.TransactionCategoryName);
            },
            record =>
            {
                Assert.Equal(10, record.TransactionValue);
                Assert.NotEqual(Guid.Empty, record.TransactionExternalId);
                Assert.Equal(existingCategory.ExternalId, record.TransactionCategoryExternalId);
                Assert.Equal(existingCategory.CategoryName, record.TransactionCategoryName);
            },
            record =>
            {
                Assert.Equal(20, record.TransactionValue);
                Assert.NotEqual(Guid.Empty, record.TransactionExternalId);
                Assert.Equal(existingCategory.ExternalId, record.TransactionCategoryExternalId);
                Assert.Equal(existingCategory.CategoryName, record.TransactionCategoryName);
            }
        );

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(requestExistingUser.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionCollectionRepositoryMock.Verify(
            repo => repo.GetUserCollectionByExternalId(requestExistingUser.Id, existingCollection.ExternalId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.GetAllUserTransactionsByCollection(requestExistingUser.Id, existingCollection.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
