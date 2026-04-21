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

public class UpdateAllUserCategoriesUseCaseTest
{
    private readonly Mock<ITransactionRecordCategoryRepository> _transactionRecordCategoryRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<AddTransactionRecordCategoryRequestDto>> _addTransactioncCategoryValidatorMock;
    private readonly Mock<IValidator<UpdateTransactionRecordCategoryRequestDto>> _updateTransactionCategoryValidatorMock;
    private readonly Mock<IValidator<List<UpdateTransactionRecordCategoryRequestDto>>> _updateTransactionrecordCategoriesValidatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly TransactionRecordCategoryService _sut;

    public UpdateAllUserCategoriesUseCaseTest()
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
    public async Task UpdateAllUserCategories_WhenCategoriesDoNotMatch_ShouldReturnInvalidArgsError()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();

        Guid category1ExternalId = Guid.NewGuid();
        Guid category2ExternalId = Guid.NewGuid();
        Guid category3ExternalId = Guid.NewGuid();

        string category1Name = "Health";
        string category2Name = "Education";
        string category3Name = "Fitness";

        var request = new List<UpdateTransactionRecordCategoryRequestDto>
        {
            new UpdateTransactionRecordCategoryRequestDto
            {
                CategoryExternalId = category1ExternalId.ToString(),
                CategoryName = category1Name
            },
            new UpdateTransactionRecordCategoryRequestDto
            {
                CategoryExternalId = category2ExternalId.ToString(),
                CategoryName = category2Name
            },
            new UpdateTransactionRecordCategoryRequestDto
            {
                CategoryExternalId = category3ExternalId.ToString(),
                CategoryName = category3Name
            }
        };

        var existingCategories = new List<TransactionRecordCategory>
        {
            new TransactionRecordCategory
            {
                ExternalId = Guid.NewGuid(),
                CategoryName = category1Name
            },
            new TransactionRecordCategory
            {
                ExternalId = category2ExternalId,
                CategoryName = category2Name
            },
            new TransactionRecordCategory
            {
                ExternalId = category3ExternalId,
                CategoryName = category3Name
            }
        };

        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserExternalId
        };

        List<Guid> categoryIds = request
            .Select(c => Guid.Parse(c.CategoryExternalId))
            .ToList();

        List<Guid>? capturedIds = null;

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
        .Callback<long, List<Guid>, CancellationToken>((_, ids, _) => capturedIds = ids)
        .ReturnsAsync(existingCategories);

        // Act
        var result = await _sut.UpdateAllUserTransactionCategories(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(TransactionRecordCategoryErrors.InvalidArgs);

        capturedIds.Should().HaveCount(3);
        capturedIds.Should().OnlyContain(id => request.Any(r => r.CategoryExternalId == id.ToString()));

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
    public async Task UpdateAllUserCategories_WhenRequestIsValid_ShouldReturnAffectedRows()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();

        Guid category1ExternalId = Guid.NewGuid();
        Guid category2ExternalId = Guid.NewGuid();
        Guid category3ExternalId = Guid.NewGuid();

        string category1Name = "Health";
        string category2Name = "Education";
        string category3Name = "Fitness";

        var request = new List<UpdateTransactionRecordCategoryRequestDto>
        {
            new UpdateTransactionRecordCategoryRequestDto
            {
                CategoryExternalId = category1ExternalId.ToString(),
                CategoryName = category1Name
            },
            new UpdateTransactionRecordCategoryRequestDto
            {
                CategoryExternalId = category2ExternalId.ToString(),
                CategoryName = category2Name
            },
            new UpdateTransactionRecordCategoryRequestDto
            {
                CategoryExternalId = category3ExternalId.ToString(),
                CategoryName = category3Name
            }
        };

        var existingCategories = new List<TransactionRecordCategory>
        {
            new TransactionRecordCategory
            {
                ExternalId = category1ExternalId,
                CategoryName = category1Name
            },
            new TransactionRecordCategory
            {
                ExternalId = category2ExternalId,
                CategoryName = category2Name
            },
            new TransactionRecordCategory
            {
                ExternalId = category3ExternalId,
                CategoryName = category3Name
            }
        };

        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserExternalId
        };

        List<Guid> categoryIds = request
            .Select(c => Guid.Parse(c.CategoryExternalId))
            .ToList();

        List<Guid>? capturedIds = null;

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _transactionRecordCategoryRepositoryMock
            .Setup(r => r.GetUserCategoriesByExternalIds(
                It.IsAny<long>(),
                It.IsAny<List<Guid>>(),
                It.IsAny<CancellationToken>()))
            .Callback<long, List<Guid>, CancellationToken>((_, ids, _) => capturedIds = ids)
            .ReturnsAsync(existingCategories);

        _transactionRecordCategoryRepositoryMock.Setup(
            repo => repo.SaveChanges(
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(3);

        // Act
        var result = await _sut.UpdateAllUserTransactionCategories(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        capturedIds.Should().HaveCount(3);
        capturedIds.Should().OnlyContain(id => categoryIds.Any(item => item == id));

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionRecordCategoryRepositoryMock.Verify(
            repo => repo.GetUserCategoriesByExternalIds(
                existingUser.Id, 
                categoryIds, 
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
