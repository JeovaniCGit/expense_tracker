using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
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

public class DeleteTransactionCategoryUseCaseTests
{
    private readonly Mock<ITransactionRecordCategoryRepository> _transactionRecordCategoryRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<AddTransactionRecordCategoryRequestDto>> _addTransactioncCategoryValidatorMock;
    private readonly Mock<IValidator<UpdateTransactionRecordCategoryRequestDto>> _updateTransactionCategoryValidatorMock;
    private readonly Mock<IValidator<List<UpdateTransactionRecordCategoryRequestDto>>> _updateTransactionrecordCategoriesValidatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly TransactionRecordCategoryService _sut;

    public DeleteTransactionCategoryUseCaseTests()
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
    public async Task DeleteTransactionCategory_WhenCategoryDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();

        string requestExternalId = Guid.NewGuid().ToString();

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.GetTransactionsCategoryByExternalId(
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync((TransactionRecordCategory?)null);

        // Act
        var result = await _sut.DeleteTransactionRecordCategory(requestExternalId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransactionRecordCategoryErrors.NotFound);

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetTransactionsCategoryByExternalId(
                Guid.Parse(requestExternalId), 
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteTransactionCategory_WhenUserIsNotOwnerOrAdmin_ShouldReturnNotOwnerError()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();
        long existingUserId = 1;

        string requestExternalId = Guid.NewGuid().ToString();

        TransactionRecordCategory existingCategory = new TransactionRecordCategory
        {
            Id = 1,
            ExternalId = Guid.Parse(requestExternalId),
            UserId = 2
        };

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.GetTransactionsCategoryByExternalId(
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingCategory);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(new User
        {
            Id = existingUserId,
            RoleId = (long)UserRoleEnum.RegularUser
        });

        // Act
        var result = await _sut.DeleteTransactionRecordCategory(requestExternalId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransactionRecordCategoryErrors.NotOwner);

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetTransactionsCategoryByExternalId(
                Guid.Parse(requestExternalId), 
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId, 
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteTransactionCategory_WhenRequestIsValid_ShouldReturnAffectedRows()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();

        string requestExternalId = Guid.NewGuid().ToString();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = Guid.Parse(requestExternalId)
        };

        TransactionRecordCategory existingCategory = new TransactionRecordCategory
        {
            Id = 1,
            ExternalId = Guid.Parse(requestExternalId),
            UserId = existingUser.Id,
        };

        TransactionRecordCategory? capturedCategory = null;

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.GetTransactionsCategoryByExternalId(
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingCategory);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        //Add callback here
        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.DeleteTransactionCategory(
                It.IsAny<TransactionRecordCategory>(),
                It.IsAny<CancellationToken>()))
        .Callback<TransactionRecordCategory, CancellationToken>((category, _) => capturedCategory = category)
        .ReturnsAsync(1);

        // Act
        var result = await _sut.DeleteTransactionRecordCategory(requestExternalId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(1);

        capturedCategory?.ExternalId.Should().Be(existingCategory.ExternalId);
        capturedCategory?.UserId.Should().Be(existingUser.Id);

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetTransactionsCategoryByExternalId(
                Guid.Parse(requestExternalId), 
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId, 
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
