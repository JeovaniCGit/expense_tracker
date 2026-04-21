using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Collections.Contracts.Requests;
using ExpenseTracker.Application.Collections.Services;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Collection.Repository;
using ExpenseTracker.Domain.Collections.Entity;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace ExpenseTracker.UnitTests.Collections;

public class GetAllUserCollectionsUseCaseTests
{
    private readonly Mock<ITransactionCollectionRepository> _transactionCollectionRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<AddCollectionRequestDto>> _addCollectionValidatorMock;
    private readonly Mock<IValidator<UpdateCollectionRequestDto>> _updateCollectionValidatorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CollectionService _sut;

    public GetAllUserCollectionsUseCaseTests()
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
    public async Task GetAllUserCollections_WhenRequestIsValid_ShouldReturnEnumerableOfCollectionResponseDto()
    {
        // Arrange
        Guid currentUserexternalId = Guid.NewGuid();

        User existingUser = new User
        {
            Id = 1,
            ExternalId = currentUserexternalId
        };

        var requestFixedStartTimestamp = new DateTimeOffset(2024, 3, 15, 10, 30, 0, TimeSpan.Zero);
        var requestFixedEndTimestamp = new DateTimeOffset(2024, 4, 15, 10, 30, 0, TimeSpan.Zero);

        IEnumerable<TransactionCollection> existingCollections = new List<TransactionCollection>
        {
            new TransactionCollection
            {
                Id = 1,
                ExternalId = Guid.NewGuid(),
                StartDate = requestFixedStartTimestamp,
                EndDate = requestFixedEndTimestamp,
                Description = "January 2nd week",
                EstimatedBudget = 100,
                RealBudget = 80,
                UserId = existingUser.Id
            },
            new TransactionCollection
            {
                Id = 2,
                ExternalId = Guid.NewGuid(),
                StartDate = requestFixedStartTimestamp,
                EndDate = requestFixedEndTimestamp,
                Description = "January 1st week",
                EstimatedBudget = 200,
                RealBudget = 180,
                UserId = existingUser.Id
            }
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
            repo => repo.GetAllUserCollections(
                It.IsAny<long>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingCollections);

        // Act
        var result = await _sut.GetAllUserCollections(requestFixedStartTimestamp, requestFixedEndTimestamp, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();

        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(c => existingCollections.Any(item => item.Description == c.Description));
        result.Value.Should().OnlyContain(c => existingCollections.Any(item => item.ExternalId == c.CollectionExternalId));
        result.Value.Should().OnlyContain(c => existingCollections.Any(item => item.EstimatedBudget == c.EstimatedBudget));
        result.Value.Should().OnlyContain(c => existingCollections.Any(item => item.RealBudget == c.RealBudget));
        result.Value.Should().OnlyContain(c => existingCollections.Any(item => item.StartDate == c.StartDate));
        result.Value.Should().OnlyContain(c => existingCollections.Any(item => item.EndDate == c.EndDate));

        _userRepositoryMock.Verify(
            repo => repo.GetUserByExternalId(
                currentUserexternalId,
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        _transactionCollectionRepositoryMock.Verify(
            repo => repo.GetAllUserCollections(
                existingUser.Id,
                requestFixedStartTimestamp,
                requestFixedEndTimestamp,
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
