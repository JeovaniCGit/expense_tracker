using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Categories.Contracts.Requests;
using ExpenseTracker.Application.Categories.Services;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Domain.Categories.Repository;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace ExpenseTrackerApplication.Tests.Categories;

public class GetUserTransactionCategoriesUseCaseTests
{
    private readonly Mock<ITransactionRecordCategoryRepository> _transactionRecordCategoryRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<AddTransactionRecordCategoryRequestDto>> _addTransactioncCategoryValidatorMock;
    private readonly Mock<IValidator<UpdateTransactionRecordCategoryRequestDto>> _updateTransactionCategoryValidatorMock;
    private readonly Mock<IValidator<List<UpdateTransactionRecordCategoryRequestDto>>> _updateTransactionrecordCategoriesValidatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly TransactionRecordCategoryService _sut;

    public GetUserTransactionCategoriesUseCaseTests()
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
    public async Task GetUserTransactionCategories_WhenRequestIsValid_ShouldReturnIEnumerableOfGetTransactionRecordCategoryResponseDto()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();

        Guid category1ExternalId = Guid.NewGuid();
        Guid category2ExternalId = Guid.NewGuid();
        Guid category3ExternalId = Guid.NewGuid();

        string category1Name = "Health";
        string category2Name = "Education";
        string category3Name = "Fitness";

        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserExternalId
        };

        IEnumerable<TransactionRecordCategory> expectedCategories = new List<TransactionRecordCategory>
        {
            new TransactionRecordCategory
            {
                Id = 1,
                ExternalId = category1ExternalId,
                CategoryName = category1Name,
                UserId = existingUser.Id
            },
            new TransactionRecordCategory
            {
                Id = 2,
                ExternalId = category2ExternalId,
                CategoryName = category2Name,
                UserId = existingUser.Id
            },
            new TransactionRecordCategory
            {
                Id = 3,
                ExternalId = category3ExternalId,
                CategoryName = category3Name,
                UserId = existingUser.Id
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
            repo => repo.GetAllUserTransactionCategories(
                It.IsAny<long>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedCategories);

        // Act
        var result = await _sut.GetAllUserTransactionCategories(CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(3);
        result.Value.Should().OnlyContain(c => expectedCategories.Any(ec => ec.CategoryName == c.CategoryName));
        result.Value.Should().OnlyContain(c => expectedCategories.Any(ec => ec.ExternalId == c.CategoryExternalId));

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId,
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
}
