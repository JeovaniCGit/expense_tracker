using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Collections.Contracts.Requests;
using ExpenseTracker.Application.Collections.Errors;
using ExpenseTracker.Application.Collections.Services;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Collection.Repository;
using ExpenseTracker.Domain.Collections.Entity;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace ExpenseTracker.UnitTests.Collections;

public class UpdateCollectionUseCaseTests
{
    private readonly Mock<ITransactionCollectionRepository> _transactionCollectionRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<AddCollectionRequestDto>> _addCollectionValidatorMock;
    private readonly Mock<IValidator<UpdateCollectionRequestDto>> _updateCollectionValidatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CollectionService _sut;

    public UpdateCollectionUseCaseTests()
    {
        _transactionCollectionRepositoryMock = new Mock<ITransactionCollectionRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _addCollectionValidatorMock = new Mock<IValidator<AddCollectionRequestDto>>();
        _updateCollectionValidatorMock = new Mock<IValidator<UpdateCollectionRequestDto>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _sut = new CollectionService(
                _transactionCollectionRepositoryMock.Object,
                _userRepositoryMock.Object,
                _addCollectionValidatorMock.Object,
                _updateCollectionValidatorMock.Object,
                _currentUserServiceMock.Object
            );
    }

    [Fact]
    public async Task UpdateCollection_WhenCollectionDoesNotExist_ShouldReturnInvalidArgsError()
    {
        // Arrange
        Guid currentUserexternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserexternalId
        };

        UpdateCollectionRequestDto request = new UpdateCollectionRequestDto
        {
            CollectionExternalId = Guid.NewGuid().ToString(),
            Description = "January",
            EstimatedBudget = 100,
            RealBudget = 80
        };

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserexternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _transactionCollectionRepositoryMock.Setup(
            repo => repo.GetCollectionByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync((TransactionCollection?)null);

        // Act
        var result = await _sut.UpdateCollection(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(CollectionErrors.InvalidArgs);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserexternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionCollectionRepositoryMock.Verify(
            repo => repo.GetCollectionByExternalId(
                Guid.Parse(request.CollectionExternalId),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateCollection_WhenUserIsNotOwner_ShouldReturnNotOwnerError()
    {
        // Arrange
        Guid currentUserexternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserexternalId
        };

        UpdateCollectionRequestDto request = new UpdateCollectionRequestDto
        {
            CollectionExternalId = Guid.NewGuid().ToString(),
            Description = "January",
            EstimatedBudget = 100,
            RealBudget = 80
        };

        var fixedStartTimestamp = new DateTimeOffset(2024, 3, 15, 10, 30, 0, TimeSpan.Zero);
        var fixedEndTimestamp = new DateTimeOffset(2024, 4, 15, 10, 30, 0, TimeSpan.Zero);

        TransactionCollection existingCollection = new TransactionCollection
        {
            Id = 1,
            ExternalId = Guid.Parse(request.CollectionExternalId),
            StartDate = fixedStartTimestamp,
            EndDate = fixedEndTimestamp,
            Description = "January",
            EstimatedBudget = 100,
            RealBudget = 80,
            UserId = 2
        };

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserexternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _transactionCollectionRepositoryMock.Setup(
            repo => repo.GetCollectionByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingCollection);

        // Act
        var result = await _sut.UpdateCollection(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(CollectionErrors.NotOwner);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserexternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionCollectionRepositoryMock.Verify(
            repo => repo.GetCollectionByExternalId(
                Guid.Parse(request.CollectionExternalId),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateCollection_WhenRequestIsValid_ShouldReturnAffectedRows()
    {
        // Arrange
        Guid currentUserexternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserexternalId
        };

        UpdateCollectionRequestDto request = new UpdateCollectionRequestDto
        {
            CollectionExternalId = Guid.NewGuid().ToString(),
            Description = "January",
            EstimatedBudget = 100,
            RealBudget = 80
        };

        var fixedStartTimestamp = new DateTimeOffset(2024, 3, 15, 10, 30, 0, TimeSpan.Zero);
        var fixedEndTimestamp = new DateTimeOffset(2024, 4, 15, 10, 30, 0, TimeSpan.Zero);

        TransactionCollection existingCollection = new TransactionCollection
        {
            Id = 1,
            ExternalId = Guid.Parse(request.CollectionExternalId),
            StartDate = fixedStartTimestamp,
            EndDate = fixedEndTimestamp,
            Description = "January",
            EstimatedBudget = 100,
            RealBudget = 80,
            UserId = existingUser.Id
        };

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserexternalId);

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _transactionCollectionRepositoryMock.Setup(
            repo => repo.GetCollectionByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingCollection);

        _transactionCollectionRepositoryMock.Setup(
            repo => repo.SaveChanges(
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateCollection(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(1);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserexternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionCollectionRepositoryMock.Verify(
            repo => repo.GetCollectionByExternalId(
                Guid.Parse(request.CollectionExternalId),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
