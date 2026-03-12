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
using Microsoft.AspNetCore.Http;
using Moq;

namespace ExpenseTrackerApplication.Tests.Records;

public class UpdateAllUserTransactionsUseCaseTests
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

    public UpdateAllUserTransactionsUseCaseTests()
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
    public async Task UpdateAllUserTransactions_WhenRecordsDoNotMatch_ShouldReturnInvalidArgsError()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();
        Guid existingCategoriesExternalId = Guid.NewGuid();

        Guid record1ExternalId = Guid.NewGuid();
        Guid record2ExternalId = Guid.NewGuid();
        Guid record3ExternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserExternalId
        };

        List<UpdateTransactionRecordRequestDto> request = new List<UpdateTransactionRecordRequestDto>()
        {
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = record1ExternalId.ToString(),
                TransactionCategoryExternalId = existingCategoriesExternalId.ToString(),
                TransactionValue = 10
            },
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = record2ExternalId.ToString(),
                TransactionCategoryExternalId = existingCategoriesExternalId.ToString(),
                TransactionValue = 100
            },
            new UpdateTransactionRecordRequestDto
            {
                TransactionExternalId = record3ExternalId.ToString(),
                TransactionCategoryExternalId = existingCategoriesExternalId.ToString(),
                TransactionValue = 20
            }
        };

        List<Guid> recordsExternalIds = request
            .Select(r => Guid.Parse(r.TransactionExternalId))
            .ToList();

        List<Guid>? capturedIds = null;

        IEnumerable<TransactionRecord> missingRecordsForErrorCheck = new List<TransactionRecord>()
        {
            new TransactionRecord
            {
                ExternalId = record1ExternalId,
                TransactionValue = 10
            },
            new TransactionRecord
            {
                ExternalId = record2ExternalId,
                TransactionValue = 100
            },
            new TransactionRecord
            {
                ExternalId = Guid.NewGuid(),
                TransactionValue = 20
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
            repo => repo.GetUserTransactionsByExternalId(
                It.IsAny<long>(), 
                It.IsAny<List<Guid>>(), 
                It.IsAny<CancellationToken>()))
        .Callback<long, List<Guid>, CancellationToken>((_, ids, _) => capturedIds = ids)
        .ReturnsAsync(missingRecordsForErrorCheck);

        // Act
        var result = await _sut.UpdateAllUserTransactions(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransactionRecordErrors.InvalidArgs);

        capturedIds.Should().HaveCount(3);
        capturedIds.Should().OnlyContain(ids => request.Any(item => item.TransactionExternalId == ids.ToString()));

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId, 
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.GetUserTransactionsByExternalId(
                existingUser.Id, 
                recordsExternalIds, 
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateAllUserTransactions_WhenCategoriesDoNotMatch_ShouldReturnInvalidArgsError()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();
        long existingUserId = 1;

        Guid healthCategoryExternalId = Guid.NewGuid();
        Guid educationCategoryExternalId = Guid.NewGuid();
        Guid gymCategoryExternalId = Guid.NewGuid();

        Guid record1ExternalId = Guid.NewGuid();
        Guid record2ExternalId = Guid.NewGuid();
        Guid record3ExternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserExternalId
        };

        var existingCategories = new List<TransactionRecordCategory>()
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

        var request = new List<UpdateTransactionRecordRequestDto>()
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
                TransactionCategoryExternalId = Guid.NewGuid().ToString(),
                TransactionValue = 20
            }
        };

        var existingRecords = new List<TransactionRecord>()
        {
            new TransactionRecord
            {
                Id = 1,
                ExternalId = record1ExternalId,
                TransactionValue = 5,
                TransactionUserId = existingUserId,
                TransactionCategoryId = 1
            },
            new TransactionRecord
            {
                Id = 2,
                ExternalId = record2ExternalId,
                TransactionValue = 10,
                TransactionUserId = existingUserId,
                TransactionCategoryId = 2
            },
            new TransactionRecord
            {
                Id = 3,
                ExternalId = record3ExternalId,
                TransactionValue = 20,
                TransactionUserId = existingUserId,
                TransactionCategoryId = 3
            }
        };

        List<Guid> recordsExternalIds = request
            .Select(r => Guid.Parse(r.TransactionExternalId))
            .ToList();

        List<Guid> categoriesExternalIds = request
            .Select(r => Guid.Parse(r.TransactionCategoryExternalId))
            .ToList();

        List<Guid>? capturedRecordIds = null;
        List<Guid>? capturedCategoryIds = null;

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _transactionRecordRepositoryMock.Setup(
            repo => repo.GetUserTransactionsByExternalId(
                It.IsAny<long>(), 
                It.IsAny<List<Guid>>(), 
                It.IsAny<CancellationToken>()))
        .Callback<long, List<Guid>, CancellationToken>((_, ids, _) => capturedRecordIds = ids)
        .ReturnsAsync(existingRecords);

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.GetUserCategoriesByExternalIds(
                It.IsAny<long>(),
                It.IsAny<List<Guid>>(), 
                It.IsAny<CancellationToken>()))
        .Callback<long, List<Guid>, CancellationToken>((_, ids, _) => capturedCategoryIds = ids)
        .ReturnsAsync(existingCategories);

        // Act
        var result = await _sut.UpdateAllUserTransactions(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransactionRecordErrors.InvalidArgs);

        capturedRecordIds.Should().HaveCount(3);
        capturedRecordIds.Should().OnlyContain(id => request.Exists(r => r.TransactionExternalId == id.ToString()));

        capturedCategoryIds.Should().HaveCount(3);
        capturedCategoryIds.Should().OnlyContain(id => request.Exists(r => r.TransactionCategoryExternalId == id.ToString()));

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId, 
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.GetUserTransactionsByExternalId(
                existingUser.Id, 
                recordsExternalIds, 
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetUserCategoriesByExternalIds(
                existingUser.Id, 
                categoriesExternalIds, 
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateAllUserTransactions_WhenRequestIsValid_ShouldReturnAffectedRows()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();
        long existingUserId = 1;

        Guid healthCategoryExternalId = Guid.NewGuid();
        Guid educationCategoryExternalId = Guid.NewGuid();
        Guid gymCategoryExternalId = Guid.NewGuid();

        Guid record1ExternalId = Guid.NewGuid();
        Guid record2ExternalId = Guid.NewGuid();
        Guid record3ExternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserExternalId
        };

        var existingCategories = new List<TransactionRecordCategory>()
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

        var request = new List<UpdateTransactionRecordRequestDto>()
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

        var existingRecords = new List<TransactionRecord>()
        {
            new TransactionRecord
            {
                Id = 1,
                ExternalId = record1ExternalId,
                TransactionValue = 5,
                TransactionUserId = existingUserId,
                TransactionCategoryId = 1
            },
            new TransactionRecord
            {
                Id = 2,
                ExternalId = record2ExternalId,
                TransactionValue = 10,
                TransactionUserId = existingUserId,
                TransactionCategoryId = 2
            },
            new TransactionRecord
            {
                Id = 3,
                ExternalId = record3ExternalId,
                TransactionValue = 20,
                TransactionUserId = existingUserId,
                TransactionCategoryId = 3
            }
        };

        List<Guid> recordsExternalIds = request
            .Select(r => Guid.Parse(r.TransactionExternalId))
            .ToList();

        List<Guid> categoriesExternalIds = request
            .Select(r => Guid.Parse(r.TransactionCategoryExternalId))
            .ToList();

        List<Guid>? capturedRecordIds = null;
        List<Guid>? capturedCategoryIds = null;

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _transactionRecordRepositoryMock.Setup(
            repo => repo.GetUserTransactionsByExternalId(
                It.IsAny<long>(),
                It.IsAny<List<Guid>>(),
                It.IsAny<CancellationToken>()))
        .Callback<long, List<Guid>, CancellationToken>((_, ids, _) => capturedRecordIds = ids)
        .ReturnsAsync(existingRecords);

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.GetUserCategoriesByExternalIds(
                It.IsAny<long>(),
                It.IsAny<List<Guid>>(),
                It.IsAny<CancellationToken>()))
        .Callback<long, List<Guid>, CancellationToken>((_, ids, _) => capturedCategoryIds = ids)
        .ReturnsAsync(existingCategories);

        _transactionRecordRepositoryMock.Setup(
            repo => repo.SaveChanges(
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(request.Count());

        // Act
        var result = await _sut.UpdateAllUserTransactions(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(request.Count());

        capturedRecordIds.Should().HaveCount(3);
        capturedRecordIds.Should().OnlyContain(id => request.Exists(r => r.TransactionExternalId == id.ToString()));

        capturedCategoryIds.Should().HaveCount(3);
        capturedCategoryIds.Should().OnlyContain(id => request.Exists(r => r.TransactionCategoryExternalId == id.ToString()));

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.GetUserTransactionsByExternalId(
                existingUser.Id,
                recordsExternalIds,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetUserCategoriesByExternalIds(
                existingUser.Id,
                categoriesExternalIds,
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