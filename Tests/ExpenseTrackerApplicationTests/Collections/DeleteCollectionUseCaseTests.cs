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

namespace ExpenseTrackerApplication.Tests.Collections;

public class DeleteCollectionUseCaseTests
{
    private readonly Mock<ITransactionCollectionRepository> _transactionCollectionRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<AddCollectionRequestDto>> _addCollectionValidatorMock;
    private readonly Mock<IValidator<UpdateCollectionRequestDto>> _updateCollectionValidatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CollectionService _sut;

    public DeleteCollectionUseCaseTests()
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
    public async Task DeleteCollection_WhenCollectionDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();
        string requestCollectionExternalId = Guid.NewGuid().ToString();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserExternalId
        };

        TransactionCollection existingCollection = new TransactionCollection
        {
            Id = 1,
            ExternalId = Guid.NewGuid()
        };

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

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
        var result = await _sut.DeleteCollection(requestCollectionExternalId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(CollectionErrors.NotFound);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionCollectionRepositoryMock.Verify(
            repo => repo.GetCollectionByExternalId(
                Guid.Parse(requestCollectionExternalId),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteCollection_WhenUserIsNotOwnerOrAdmin_ShouldReturnNotOwnerError()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();
        string requestCollectionExternalId = Guid.NewGuid().ToString();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserExternalId
        };

        TransactionCollection existingCollection = new TransactionCollection
        {
            Id = 1,
            ExternalId = Guid.NewGuid(),
            UserId = 2
        };

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

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
        var result = await _sut.DeleteCollection(requestCollectionExternalId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(CollectionErrors.NotOwner);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionCollectionRepositoryMock.Verify(
            repo => repo.GetCollectionByExternalId(
                Guid.Parse(requestCollectionExternalId),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteCollection_WhenRequestIsValid_ShouldReturnAffectedRows()
    {
        // Arrange
        Guid currentUserExternalId = Guid.NewGuid();
        string requestCollectionExternalId = Guid.NewGuid().ToString();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserExternalId
        };

        TransactionCollection existingCollection = new TransactionCollection
        {
            Id = 1,
            ExternalId = Guid.NewGuid(),
            UserId = existingUser.Id
        };

        TransactionCollection? capturedCollection = null;

        _currentUserServiceMock.Setup(
            service => service.UserExternalId)
        .Returns(currentUserExternalId);

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
            repo => repo.DeleteCollection(
                It.IsAny<TransactionCollection>(),
                It.IsAny<CancellationToken>()))
        .Callback<TransactionCollection, CancellationToken>((collection, _) => capturedCollection = collection)
        .ReturnsAsync(1);

        // Act
        var result = await _sut.DeleteCollection(requestCollectionExternalId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().Be(1);

        capturedCollection.UserId.Should().Be(existingCollection.UserId);
        capturedCollection.Id.Should().Be(existingCollection.Id);
        capturedCollection.ExternalId.Should().Be(existingCollection.ExternalId);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserExternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionCollectionRepositoryMock.Verify(
            repo => repo.GetCollectionByExternalId(
                Guid.Parse(requestCollectionExternalId),
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionCollectionRepositoryMock.Verify(
            repo => repo.DeleteCollection(
                existingCollection,
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
