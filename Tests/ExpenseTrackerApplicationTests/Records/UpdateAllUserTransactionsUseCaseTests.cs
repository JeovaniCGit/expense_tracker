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
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ExpenseTrackerApplication.Tests.Records;

public class UpdateAllUserTransactionsUseCaseTests
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

    public UpdateAllUserTransactionsUseCaseTests()
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
    public async Task UpdateAllUserTransactions_WhenUserDoesNotExist_ShouldReturnInvalidArgsError()
    {
        // Arrange
        Guid requestUserExternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = Guid.NewGuid()
        };

        TransactionRecordCategory existingCategory = new TransactionRecordCategory
        {
            Id = 1,
            ExternalId = Guid.NewGuid(),
            CategoryName = "Health"
        };

        List<UpdateTransactionRecordRequestDto> request = new List<UpdateTransactionRecordRequestDto>()
        {
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = Guid.NewGuid().ToString(),
                TransactionCategoryExternalId = existingCategory.ExternalId.ToString(),
                TransactionValue = 10
            },
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = Guid.NewGuid().ToString(),
                TransactionCategoryExternalId = existingCategory.ExternalId.ToString(),
                TransactionValue = 100
            },
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = Guid.NewGuid().ToString(),
                TransactionCategoryExternalId = existingCategory.ExternalId.ToString(),
                TransactionValue = 20
            },
        };

        _currentUserServiceMock.Setup(service => service.UserId).Returns(existingUser.Id);

        _userRepositoryMock.Setup(
            repo => repo.GetUserById(
                existingUser.Id, 
                It.IsAny<CancellationToken>())
        ).ReturnsAsync((User?)null);

        // Act
        var result = await _sut.UpdateAllUserTransactions(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(TransactionRecordErrors.InvalidArgs, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(existingUser.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateAllUserTransactions_WhenThereIsRecordDuplicates_ShouldReturnInvalidArgsError()
    {
        // Arrange
        Guid requestUserExternalId = Guid.NewGuid();
        Guid existingCategoriesExternalId = Guid.NewGuid();
        Guid duplicateExternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = Guid.NewGuid()
        };

        List<UpdateTransactionRecordRequestDto> request = new List<UpdateTransactionRecordRequestDto>()
        {
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = duplicateExternalId.ToString(),
                TransactionCategoryExternalId = existingCategoriesExternalId.ToString(),
                TransactionValue = 10
            },
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = duplicateExternalId.ToString(),
                TransactionCategoryExternalId = existingCategoriesExternalId.ToString(),
                TransactionValue = 100
            },
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = Guid.NewGuid().ToString(),
                TransactionCategoryExternalId = existingCategoriesExternalId.ToString(),
                TransactionValue = 20
            },
        };

        _currentUserServiceMock.Setup(service => service.UserId).Returns(existingUser.Id);

        _userRepositoryMock.Setup(
            repo => repo.GetUserById(existingUser.Id, It.IsAny<CancellationToken>())
        ).ReturnsAsync(existingUser);

        // Act
        var result = await _sut.UpdateAllUserTransactions(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(TransactionRecordErrors.InvalidArgs, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(existingUser.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateAllUserTransactions_WhenRecordDoesNotExist_ShouldReturnInvalidArgsError()
    {
        // Arrange
        Guid requestUserExternalId = Guid.NewGuid();
        Guid existingCategoriesExternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = Guid.NewGuid()
        };

        IEnumerable<TransactionRecord> emptyRecordsForNoMatch = new List<TransactionRecord>();

        List<UpdateTransactionRecordRequestDto> request = new List<UpdateTransactionRecordRequestDto>()
        {
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = Guid.NewGuid().ToString(),
                TransactionCategoryExternalId = existingCategoriesExternalId.ToString(),
                TransactionValue = 10
            },
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = Guid.NewGuid().ToString(),
                TransactionCategoryExternalId = existingCategoriesExternalId.ToString(),
                TransactionValue = 100
            },
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = Guid.NewGuid().ToString(),
                TransactionCategoryExternalId = existingCategoriesExternalId.ToString(),
                TransactionValue = 20
            }
        };

        List<Guid> recordsExternalIds = request
            .Select(r => Guid.Parse(r.TransactionExternalId))
            .ToList();

        _currentUserServiceMock.Setup(service => service.UserId).Returns(existingUser.Id);

        _userRepositoryMock.Setup(
            repo => repo.GetUserById(existingUser.Id, It.IsAny<CancellationToken>())
        ).ReturnsAsync(existingUser);

        _transactionRecordRepositoryMock.Setup(
            repo => repo.GetUserTransactionsByExternalId(
                existingUser.Id, 
                recordsExternalIds, 
                It.IsAny<CancellationToken>())
        ).ReturnsAsync(emptyRecordsForNoMatch);

        // Act
        var result = await _sut.UpdateAllUserTransactions(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(TransactionRecordErrors.InvalidArgs, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(existingUser.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.GetUserTransactionsByExternalId(existingUser.Id, recordsExternalIds, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateAllUserTransactions_WhenCategoryDoesNotExist_ShouldReturnInvalidArgsError()
    {
        // Arrange
        Guid requestUserExternalId = Guid.NewGuid();
        Guid healthCategoryExternalId = Guid.NewGuid();
        Guid educationCategoryExternalId = Guid.NewGuid();
        Guid gymCategoryExternalId = Guid.NewGuid();

        Guid record1ExternalId = Guid.NewGuid();
        Guid record2ExternalId = Guid.NewGuid();
        Guid record3ExternalId = Guid.NewGuid();


        User existingUser = new User
        {
            Id = 1,
            ExternalId = Guid.NewGuid()
        };

        IEnumerable<TransactionRecordCategory> existingCategories = new List<TransactionRecordCategory>()
        {
            new TransactionRecordCategory
            {
                Id = 1,
                ExternalId = healthCategoryExternalId,
                CategoryName = "Health"
            },
            new TransactionRecordCategory
            {
                Id = 2,
                ExternalId = educationCategoryExternalId,
                CategoryName = "Education"
            },
            new TransactionRecordCategory
            {
                Id = 3,
                ExternalId = gymCategoryExternalId,
                CategoryName = "Gym"
            }
        };

        List<UpdateTransactionRecordRequestDto> request = new List<UpdateTransactionRecordRequestDto>()
        {
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = record1ExternalId.ToString(),
                TransactionCategoryExternalId = Guid.NewGuid().ToString(),
                TransactionValue = 10
            },
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = record2ExternalId.ToString(),
                TransactionCategoryExternalId = Guid.NewGuid().ToString(),
                TransactionValue = 100
            },
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = record3ExternalId.ToString(),
                TransactionCategoryExternalId = Guid.NewGuid().ToString(),
                TransactionValue = 20
            }
        };

        IEnumerable<TransactionRecord> existingRecords = new List<TransactionRecord>()
        {
            new TransactionRecord
            {
                Id = 1,
                ExternalId = record1ExternalId,
                TransactionValue = 5,
                TransactionUserId = 2,
                TransactionCategoryId = 1
            },
            new TransactionRecord
            {
                Id = 2,
                ExternalId = record2ExternalId,
                TransactionValue = 10,
                TransactionUserId = 2,
                TransactionCategoryId = 2
            },
            new TransactionRecord
            {
                Id = 3,
                ExternalId = record3ExternalId,
                TransactionValue = 20,
                TransactionUserId = 2,
                TransactionCategoryId = 1
            }
        };

        List<Guid> recordsExternalIds = request
            .Select(r => Guid.Parse(r.TransactionExternalId))
            .ToList();

        List<Guid> categoriesExternalIds = request
            .Select(r => Guid.Parse(r.TransactionCategoryExternalId))
            .ToList();

        _currentUserServiceMock.Setup(service => service.UserId).Returns(existingUser.Id);

        _userRepositoryMock.Setup(
            repo => repo.GetUserById(existingUser.Id, It.IsAny<CancellationToken>())
        ).ReturnsAsync(existingUser);

        _transactionRecordRepositoryMock.Setup(
            repo => repo.GetUserTransactionsByExternalId(
                existingUser.Id, 
                recordsExternalIds, 
                It.IsAny<CancellationToken>())
        ).ReturnsAsync(existingRecords);

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.GetUserCategoriesByExternalIds(
                existingUser.Id, 
                categoriesExternalIds, 
                It.IsAny<CancellationToken>())
        ).ReturnsAsync(existingCategories);

        // Act
        var result = await _sut.UpdateAllUserTransactions(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(TransactionRecordErrors.InvalidArgs, result.FirstError);

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(existingUser.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.GetUserTransactionsByExternalId(existingUser.Id, recordsExternalIds, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetUserCategoriesByExternalIds(existingUser.Id, categoriesExternalIds, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateAllUserTransactions_WhenRequestIsValid_ShouldReturnAffectedRows()
    {
        // Arrange
        Guid requestUserExternalId = Guid.NewGuid();
        Guid healthCategoryExternalId = Guid.NewGuid();
        Guid educationCategoryExternalId = Guid.NewGuid();
        Guid gymCategoryExternalId = Guid.NewGuid();

        Guid record1ExternalId = Guid.NewGuid();
        Guid record2ExternalId = Guid.NewGuid();
        Guid record3ExternalId = Guid.NewGuid();


        User existingUser = new User
        {
            Id = 1,
            ExternalId = Guid.NewGuid()
        };

        IEnumerable<TransactionRecordCategory> existingCategories = new List<TransactionRecordCategory>()
        {
            new TransactionRecordCategory
            {
                Id = 1,
                ExternalId = healthCategoryExternalId,
                CategoryName = "Health"
            },
            new TransactionRecordCategory
            {
                Id = 2,
                ExternalId = educationCategoryExternalId,
                CategoryName = "Education"
            },
            new TransactionRecordCategory
            {
                Id = 3,
                ExternalId = gymCategoryExternalId,
                CategoryName = "Gym"
            }
        };

        List<UpdateTransactionRecordRequestDto> request = new List<UpdateTransactionRecordRequestDto>()
        {
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = record1ExternalId.ToString(),
                TransactionCategoryExternalId = healthCategoryExternalId.ToString(),
                TransactionValue = 10
            },
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = record2ExternalId.ToString(),
                TransactionCategoryExternalId = educationCategoryExternalId.ToString(),
                TransactionValue = 100
            },
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = record3ExternalId.ToString(),
                TransactionCategoryExternalId = gymCategoryExternalId.ToString(),
                TransactionValue = 20
            }
        };

        IEnumerable<TransactionRecord> existingRecords = new List<TransactionRecord>()
        {
            new TransactionRecord
            {
                Id = 1,
                ExternalId = record1ExternalId,
                TransactionValue = 5,
                TransactionUserId = 2,
                TransactionCategoryId = 1
            },
            new TransactionRecord
            {
                Id = 2,
                ExternalId = record2ExternalId,
                TransactionValue = 10,
                TransactionUserId = 2,
                TransactionCategoryId = 2
            },
            new TransactionRecord
            {
                Id = 3,
                ExternalId = record3ExternalId,
                TransactionValue = 20,
                TransactionUserId = 2,
                TransactionCategoryId = 1
            }
        };

        List<Guid> recordsExternalIds = request
            .Select(r => Guid.Parse(r.TransactionExternalId))
            .ToList();

        List<Guid> categoriesExternalIds = request
            .Select(r => Guid.Parse(r.TransactionCategoryExternalId))
            .ToList();

        _currentUserServiceMock.Setup(service => service.UserId).Returns(existingUser.Id);

        _userRepositoryMock.Setup(
            repo => repo.GetUserById(existingUser.Id, It.IsAny<CancellationToken>())
        ).ReturnsAsync(existingUser);

        _transactionRecordRepositoryMock.Setup(
            repo => repo.GetUserTransactionsByExternalId(
                existingUser.Id, 
                recordsExternalIds, 
                It.IsAny<CancellationToken>())
        ).ReturnsAsync(existingRecords);

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.GetUserCategoriesByExternalIds(
                existingUser.Id, 
                categoriesExternalIds, 
                It.IsAny<CancellationToken>())
        ).ReturnsAsync(existingCategories);

        _transactionRecordRepositoryMock.Setup(
            repo => repo.UpdateAllUserTransactions(
                It.IsAny<List<TransactionRecord>>(), 
                It.IsAny<CancellationToken>())
        ).ReturnsAsync(It.IsAny<int>());

        // Act
        var result = await _sut.UpdateAllUserTransactions(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(typeof(int), result.Value.GetType());

        _userRepositoryMock.Verify(
            repo => repo.GetUserById(existingUser.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.GetUserTransactionsByExternalId(existingUser.Id, recordsExternalIds, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetUserCategoriesByExternalIds(existingUser.Id, categoriesExternalIds, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.UpdateAllUserTransactions(It.IsAny<List<TransactionRecord>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}