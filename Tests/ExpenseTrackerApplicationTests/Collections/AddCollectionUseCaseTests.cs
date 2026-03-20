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

public class AddCollectionUseCaseTests
{
    private readonly Mock<ITransactionCollectionRepository> _transactionCollectionRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<AddCollectionRequestDto>> _addCollectionValidatorMock;
    private readonly Mock<IValidator<UpdateCollectionRequestDto>> _updateCollectionValidatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CollectionService _sut;

    public AddCollectionUseCaseTests()
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
    public async Task AddCollection_WhenUserDoesNotExist_ShouldReturnInvalidArgsError()
    {
        // Arrange
        var fixedStartTimestamp = new DateTimeOffset(2024, 3, 15, 10, 30, 0, TimeSpan.Zero);
        var fixedEndTimestamp = new DateTimeOffset(2024, 4, 15, 10, 30, 0, TimeSpan.Zero);

        var request = new AddCollectionRequestDto
        {
            Description = "some-description",
            UserExternalId = Guid.NewGuid().ToString(),
            EstimatedBudget = 5,
            RealBudget = 2,
            StartDate = fixedStartTimestamp,
            EndDate = fixedEndTimestamp
        };

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.AddCollection(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(CollectionErrors.InvalidArgs);

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                Guid.Parse(request.UserExternalId),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task AddCollection_WhenRequestIsValid_ShouldReturnAddCollectionResponseDto()
    {
        // Arrange
        var fixedStartTimestamp = new DateTimeOffset(2024, 3, 15, 10, 30, 0, TimeSpan.Zero);
        var fixedEndTimestamp = new DateTimeOffset(2024, 4, 15, 10, 30, 0, TimeSpan.Zero);
        var fixedCreatedAtTimestamp = new DateTimeOffset(2024, 3, 15, 10, 30, 0, TimeSpan.Zero);

        var request = new AddCollectionRequestDto
        {
            Description = "some-description",
            UserExternalId = Guid.NewGuid().ToString(),
            EstimatedBudget = 5,
            RealBudget = 2,
            StartDate = fixedStartTimestamp,
            EndDate = fixedEndTimestamp
        };

        TransactionCollection? capturedCollection = null;

        TransactionCollection addedColelction = new TransactionCollection
        {
            ExternalId = Guid.NewGuid(),
            Description = request.Description,
            CreatedAt = fixedCreatedAtTimestamp
        };

        User existingUser = new User
        {
            Id = 1,
            ExternalId = Guid.Parse(request.UserExternalId)
        };

        _userRepositoryMock.Setup(
            repo => repo.GetUserByExternalId(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingUser);

        _transactionCollectionRepositoryMock.Setup(
            repo => repo.AddCollection(
                It.IsAny<TransactionCollection>(),
                It.IsAny<CancellationToken>()))
        .Callback<TransactionCollection, CancellationToken>((collection, _) => capturedCollection = collection)
        .ReturnsAsync(addedColelction);

        // Act
        var result = await _sut.AddCollection(request, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.ExternalId.Should().Be(addedColelction.ExternalId);
        result.Value.CreatedAt.Should().Be(addedColelction.CreatedAt);
        result.Value.Description.Should().Be(addedColelction.Description);

        capturedCollection.Description.Should().Be(request.Description);
        capturedCollection.UserId.Should().Be(existingUser.Id);
        capturedCollection.EstimatedBudget.Should().Be(request.EstimatedBudget);
        capturedCollection.RealBudget.Should().Be(request.RealBudget);
        capturedCollection.StartDate.Should().Be(request.StartDate);
        capturedCollection.EndDate.Should().Be(request.EndDate);


        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                Guid.Parse(request.UserExternalId),
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionCollectionRepositoryMock.Verify(
            repo => repo.AddCollection(
                It.IsAny<TransactionCollection>(),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
