using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Categories.Contracts.Requests;
using ExpenseTracker.Application.Categories.Contracts.Responses;
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

public class AddTransactionCategoryUseCaseTests
{
    private readonly Mock<ITransactionRecordCategoryRepository> _transactionRecordCategoryRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<AddTransactionRecordCategoryRequestDto>> _addTransactioncCategoryValidatorMock;
    private readonly Mock<IValidator<UpdateTransactionRecordCategoryRequestDto>> _updateTransactionCategoryValidatorMock;
    private readonly Mock<IValidator<List<UpdateTransactionRecordCategoryRequestDto>>> _updateTransactionrecordCategoriesValidatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly TransactionRecordCategoryService _sut;

    public AddTransactionCategoryUseCaseTests()
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
    public async Task AddTransactionCategory_WhenUserDoesNotExist_ShouldReturnInvalidArgsError()
    {
        // Arrange
        var request = new AddTransactionRecordCategoryRequestDto
        {
            CategoryName = "Education",
            UserExternalId = Guid.NewGuid().ToString()
        };

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
               It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>())
        ).ReturnsAsync((User?)null);

        // Act
        var result = await _sut.AddUserTransactionRecordCategory(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransactionRecordCategoryErrors.InvalidArgs);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                Guid.Parse(request.UserExternalId), 
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task AddTransactionCategory_WhenRequestIsValid_ShouldReturnAddTransactionRecordCategoryResponseDto()
    {
        // Arrange
        var request = new AddTransactionRecordCategoryRequestDto
        {
            CategoryName = "Education",
            UserExternalId = Guid.NewGuid().ToString()
        };

        User existingUser = new User
        {
            Id = 1,
            ExternalId = Guid.Parse(request.UserExternalId)
        };

        var fixedCreatedAtTimestamp = new DateTimeOffset(2024, 1, 15, 8, 0, 0, TimeSpan.Zero);

        TransactionRecordCategory addedcategory = new TransactionRecordCategory
        {
            CategoryName = request.CategoryName,
            ExternalId = Guid.NewGuid(),
            CreatedAt = fixedCreatedAtTimestamp
        };

        TransactionRecordCategory? capturedcategory = null;

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.GetUserCategoryByCategoryName(
                It.IsAny<long>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
        .ReturnsAsync((TransactionRecordCategory?)null);

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.AddTransactionCategory(
                It.IsAny<TransactionRecordCategory>(), 
                It.IsAny<CancellationToken>()))
        .Callback<TransactionRecordCategory, CancellationToken>((category, _) => capturedcategory = category)
        .ReturnsAsync(addedcategory);

        // Act
        var result = await _sut.AddUserTransactionRecordCategory(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.CategoryName.Should().Be(request.CategoryName);
        result.Value.CategoryExternalId.Should().NotBe(Guid.Empty);
        result.Value.CreatedAt.Should().Be(fixedCreatedAtTimestamp);

        capturedcategory?.CategoryName.Should().Be(request.CategoryName);
        capturedcategory?.UserId.Should().Be(existingUser.Id);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                Guid.Parse(request.UserExternalId), 
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.AddTransactionCategory(
                It.IsAny<TransactionRecordCategory>(),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
