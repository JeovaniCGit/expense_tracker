using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Categories.Contracts.Requests;
using ExpenseTracker.Application.Categories.Errors;
using ExpenseTracker.Application.Categories.Services;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Domain.Categories.Repository;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace ExpenseTracker.UnitTests.Categories;

public class UpdateUserTransactionCategoryUseCaseTests
{
    private readonly Mock<ITransactionRecordCategoryRepository> _transactionRecordCategoryRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<AddTransactionRecordCategoryRequestDto>> _addTransactioncCategoryValidatorMock;
    private readonly Mock<IValidator<UpdateTransactionRecordCategoryRequestDto>> _updateTransactionCategoryValidatorMock;
    private readonly Mock<IValidator<List<UpdateTransactionRecordCategoryRequestDto>>> _updateTransactionrecordCategoriesValidatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly TransactionRecordCategoryService _sut;

    public UpdateUserTransactionCategoryUseCaseTests()
    {
        _transactionRecordCategoryRepositoryMock = new Mock<ITransactionRecordCategoryRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _addTransactioncCategoryValidatorMock = new Mock<IValidator<AddTransactionRecordCategoryRequestDto>>();
        _updateTransactionCategoryValidatorMock = new Mock<IValidator<UpdateTransactionRecordCategoryRequestDto>>();
        _updateTransactionrecordCategoriesValidatorMock = new Mock<IValidator<List<UpdateTransactionRecordCategoryRequestDto>>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _sut = new TransactionRecordCategoryService(
            _transactionRecordCategoryRepositoryMock.Object,
            _userRepositoryMock.Object,
            _addTransactioncCategoryValidatorMock.Object,
            _updateTransactionCategoryValidatorMock.Object,
            _updateTransactionrecordCategoriesValidatorMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task UpdateUserTransactionCategory_WhenCategoryDoesNotExist_ShouldReturnInvalidArgsError()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserExternalId
        };

        UpdateTransactionRecordCategoryRequestDto request = new UpdateTransactionRecordCategoryRequestDto
        {
            CategoryExternalId = Guid.NewGuid().ToString(),
            CategoryName = "Health",
            Version = 123
        };

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.GetUserCategoriesByExternalIds(
                It.IsAny<long>(), 
                It.IsAny<List<Guid>>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<TransactionRecordCategory>());

        // Act
        var result = await _sut.UpdateUserTransactionCategory(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransactionRecordCategoryErrors.InvalidArgs);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId, 
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetUserCategoriesByExternalIds(
                It.IsAny<long>(), 
                It.IsAny<List<Guid>>(),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateUserTransactionCategory_WhenUserIsNotOwner_ShouldReturnInvalidArgsError()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserExternalId
        };

        UpdateTransactionRecordCategoryRequestDto request = new UpdateTransactionRecordCategoryRequestDto
        {
            CategoryExternalId = Guid.NewGuid().ToString(),
            CategoryName = "Health",
            Version = 123
        };

        TransactionRecordCategory existingCategory = new TransactionRecordCategory
        {
            Id = 1,
            UserId = 2,
            ExternalId = Guid.NewGuid(),
            CategoryName = "Education"
        };

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);
        
        _transactionRecordCategoryRepositoryMock.Setup(
                repo => repo.GetUserCategoriesByExternalIds(
                    It.IsAny<long>(), 
                    It.IsAny<List<Guid>>(), 
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TransactionRecordCategory>());

        // Act
        var result = await _sut.UpdateUserTransactionCategory(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransactionRecordCategoryErrors.InvalidArgs);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetUserCategoriesByExternalIds(
                It.IsAny<long>(), 
                It.IsAny<List<Guid>>(), 
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateUserTransactionCategory_WhenRequestIsValid_ShouldReturnAffectedRows()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();
        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserExternalId
        };

        UpdateTransactionRecordCategoryRequestDto request = new UpdateTransactionRecordCategoryRequestDto
        {
            CategoryExternalId = Guid.NewGuid().ToString(),
            CategoryName = "Health",
            Version = 123
        };

        var existingCategories = new List<TransactionRecordCategory>
        {
            new TransactionRecordCategory
            {
                Id = 1,
                UserId = existingUser.Id,
                ExternalId = Guid.Parse(request.CategoryExternalId),
                CategoryName = "Education"
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

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.GetUserCategoriesByExternalIds(
                It.IsAny<long>(), 
                It.IsAny<List<Guid>>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingCategories);

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.SaveChanges(
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateUserTransactionCategory(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(1);
        existingCategories.Should().HaveCount(1);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetUserCategoriesByExternalIds(
                It.IsAny<long>(), 
                It.IsAny<List<Guid>>(), 
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.SaveChanges(
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
