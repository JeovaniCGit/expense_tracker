using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Categories.Errors;
using ExpenseTracker.Application.Records.Contracts.Requests;
using ExpenseTracker.Application.Records.Contracts.Responses;
using ExpenseTracker.Application.Records.Errors;
using ExpenseTracker.Application.Records.Services;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Domain.Categories.Repository;
using ExpenseTracker.Domain.Collection.Repository;
using ExpenseTracker.Domain.Records.Entity;
using ExpenseTracker.Domain.Records.Repository;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ExpenseTrackerApplication.Tests.Records;

public class GetAllUserTransactionRecordsByCategoryUseCaseTests
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

    public GetAllUserTransactionRecordsByCategoryUseCaseTests()
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
    public async Task GetAllUserTransactionRecordsByCategory_WhenCategoryDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        long requestUserId = 1;
        Guid categoryExternalId = Guid.NewGuid();
        Guid userExternalId = Guid.NewGuid();

        User requestExistingUser = new User
        {
            Id = requestUserId,
            ExternalId = userExternalId
        };

        User existingUser = new User
        {
            Id = requestUserId,
            ExternalId = Guid.NewGuid()
        };

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(requestUserId);

        _userRepositoryMock.Setup(repo => repo.GetUserById(requestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestExistingUser);

        _transactionRecordCategoryRepositoryMock.Setup(repo => repo.GetTransactionCategoryIdByExternalId(categoryExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((long?)null);

        // Act
        var result = await _sut.GetAllUserTransactionsByCategory(categoryExternalId.ToString(), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(TransactionRecordCategoryErrors.NotFound, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(requestUserId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetTransactionCategoryIdByExternalId(categoryExternalId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAllUserTransactionRecordsByCategory_WhenRequestIsValid_ShouldReturnIEnumerableOfGetTransactionRecordResponseDto()
    {
        // Arrange
        long requestUserId = 1;
        Guid categoryExternalId = Guid.NewGuid();
        Guid requestUserExternalId = Guid.NewGuid();

        User requestExistingUser = new User
        {
            Id = requestUserId,
            ExternalId = requestUserExternalId
        };

        User existingUser = new User
        {
            Id = requestUserId,
            ExternalId = Guid.NewGuid()
        };

        TransactionRecordCategory existingCategory = new TransactionRecordCategory
        {
            Id = 1,
            ExternalId = categoryExternalId,
            CategoryName = "Health"
        };

        IEnumerable<TransactionRecord> existingRecords = new List<TransactionRecord> ()
        {
            new TransactionRecord
            {
                Id = 1,
                ExternalId= Guid.NewGuid(),
                TransactionValue = 5,
                TransactionUserId = requestUserId,
                TransactionCategoryId = existingCategory.Id,
                TransactionCategory = existingCategory
            },
            new TransactionRecord
            {
                Id = 2,
                ExternalId= Guid.NewGuid(),
                TransactionValue = 10,
                TransactionUserId = requestUserId,
                TransactionCategoryId = existingCategory.Id,
                TransactionCategory = existingCategory
            },
            new TransactionRecord
            {
                Id = 3,
                ExternalId= Guid.NewGuid(),
                TransactionValue = 20,
                TransactionUserId = requestUserId,
                TransactionCategoryId = existingCategory.Id,
                TransactionCategory = existingCategory
            },
        };

        _currentUserServiceMock.Setup(service => service.UserId)
            .Returns(requestUserId);

        _userRepositoryMock.Setup(repo => repo.GetUserById(requestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestExistingUser);

        _transactionRecordCategoryRepositoryMock.Setup(repo => repo.GetTransactionCategoryIdByExternalId(categoryExternalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory.Id);

        _transactionRecordRepositoryMock.Setup(repo => repo.GetAllUserTransactionsByCategory(existingUser.Id, existingCategory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRecords);

        // Act
        var result = await _sut.GetAllUserTransactionsByCategory(categoryExternalId.ToString(), CancellationToken.None);

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
            repo => repo.GetUserById(requestUserId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetTransactionCategoryIdByExternalId(categoryExternalId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.GetAllUserTransactionsByCategory(existingUser.Id, existingCategory.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
