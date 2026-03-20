using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
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

public class AddTransactionRecordUseCaseTests
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

    public AddTransactionRecordUseCaseTests()
    {
        _transactionRecordRepositoryMock = new Mock<ITransactionRecordRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _transactionRecordCategoryRepositoryMock = new Mock<ITransactionRecordCategoryRepository>();
        _transactionCollectionRepositoryMock = new Mock<ITransactionCollectionRepository>();
        _addRecordValidatorMock = new Mock<IValidator<AddTransactionRecordRequestDto>>();
        _updateRecordValidatorMock = new Mock<IValidator<UpdateTransactionRecordRequestDto>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _updateRecordsValidatorMock = new Mock<IValidator<List<UpdateTransactionRecordRequestDto>>>();
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
    public async Task AddTransactionRecord_WhenUserDoesNotExist_ShouldReturnInvalidArgsError()
    {
        // Arrange
        AddTransactionRecordRequestDto request = new AddTransactionRecordRequestDto
        {
            TransactionCategoryExternalId = Guid.NewGuid().ToString(),
            TransactionUserExternalId = Guid.NewGuid().ToString(),
            TransactionValue = 2,
        };

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);


        // Act
        var result = await _sut.AddUserTransactionRecord(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransactionRecordErrors.InvalidArgs);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                Guid.Parse(request.TransactionUserExternalId), 
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task AddTransactionRecord_WhenRequestCategoryIdDoesNotExist_ShouldReturnInvalidArgsError()
    {
        // Arrange
        AddTransactionRecordRequestDto request = new AddTransactionRecordRequestDto
        {
            TransactionCategoryExternalId = Guid.NewGuid().ToString(),
            TransactionUserExternalId = Guid.NewGuid().ToString(),
            TransactionValue = 2,
        };

        var existingDefaultCategories = new List<TransactionRecordCategory>()
        {
            new TransactionRecordCategory
            {
                CategoryName = "Banking",
                UserId = 2,
                ExternalId = Guid.NewGuid(),
            }
        };

        var existingUserCategories = new List<TransactionRecordCategory>()
        {
            new TransactionRecordCategory
            {
                CategoryName = "Health",
                UserId = 1,
                ExternalId = Guid.NewGuid(),
            },
            new TransactionRecordCategory
            {
                CategoryName = "Education",
                UserId = 1,
                ExternalId = Guid.NewGuid(),
            }
        };

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

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.GetAllTransactionsCategories(
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingDefaultCategories);

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.GetAllUserTransactionCategories(
                It.IsAny<long>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUserCategories);

        // Act
        var result = await _sut.AddUserTransactionRecord(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransactionRecordErrors.InvalidArgs);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                Guid.Parse(request.TransactionUserExternalId), 
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetAllTransactionsCategories(
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetAllUserTransactionCategories(
                existingUser.Id, 
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task AddTransactionRecord_WhenRequestIsValid_ShouldReturnAddTransactionRecordResponseDto()
    {
        // Arrange
        var request = new AddTransactionRecordRequestDto
        {
            TransactionCategoryExternalId = Guid.NewGuid().ToString(),
            TransactionUserExternalId = Guid.NewGuid().ToString(),
            TransactionValue = 2,
        };

        var existingDefaultCategories = new List<TransactionRecordCategory>()
        {
            new TransactionRecordCategory
            {
                CategoryName = "Banking",
                UserId = 2,
                ExternalId = Guid.NewGuid(),
            }
        };

        var existingUserCategories = new List<TransactionRecordCategory>()
        {
            new TransactionRecordCategory
            {
                Id = 1,
                CategoryName = "Health",
                UserId = 1,
                ExternalId = Guid.Parse(request.TransactionCategoryExternalId),
            },
            new TransactionRecordCategory
            {
                Id = 2,
                CategoryName = "Education",
                UserId = 1,
                ExternalId = Guid.NewGuid(),
            }
        };

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

        var fixedTimestamp = new DateTimeOffset(2024, 3, 15, 10, 30, 0, TimeSpan.Zero);

        TransactionRecord addedRecord = new TransactionRecord
        {
            TransactionValue = request.TransactionValue,
            ExternalId = Guid.NewGuid(),
            CreatedAt = fixedTimestamp
        };

        TransactionRecord? capturedRecord = null;

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.GetAllTransactionsCategories(
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(It.IsAny<IEnumerable<TransactionRecordCategory>>());

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.GetAllUserTransactionCategories(
                It.IsAny<long>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUserCategories);

        _transactionRecordRepositoryMock.Setup(repo => repo.AddTransaction(
            It.IsAny<TransactionRecord>(), 
            It.IsAny<CancellationToken>()))
        .Callback<TransactionRecord, CancellationToken>((record, _) => capturedRecord = record)
        .ReturnsAsync(addedRecord);

        // Act
        var result = await _sut.AddUserTransactionRecord(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TransactionValue.Should().Be(request.TransactionValue);
        result.Value.CreatedAt.Should().Be(fixedTimestamp);
        result.Value.ExternalId.Should().NotBe(Guid.Empty);

        capturedRecord.TransactionValue.Should().Be(request.TransactionValue);
        capturedRecord.TransactionUserId.Should().Be(existingUser.Id);
        capturedRecord.TransactionCategoryId.Should().NotBe(null);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                Guid.Parse(request.TransactionUserExternalId), 
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetAllTransactionsCategories(
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetAllUserTransactionCategories(
                existingUser.Id, 
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordRepositoryMock.Verify(
            repo => repo.AddTransaction(
                It.IsAny<TransactionRecord>(), 
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
