using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Collections.Contracts.Requests;
using ExpenseTracker.Application.Collections.Contracts.Responses;
using ExpenseTracker.Infrastructure.Database;
using ExpenseTracker.IntegrationTests.Accounts.Builder;
using ExpenseTracker.IntegrationTests.Collections.Builder;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.IntegrationTests.Collections.Tests;

public class CollectionsCrudFlowTest : BaseIntegrationTest
{
    public CollectionsCrudFlowTest(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateReadUpdateDeleteCollections_ShouldSucceed()
    {
        // Arrange - seed required data
        var userExternalId = await SeedUserData();

        var addCollectionDto = new TransactionCollectionBuilder()
            .BuildAddCollectionRequestDto(userExternalId.ToString());

        // Act - create
        
        // Simulate authentication by adding required headers (in a real scenario, we would obtain a JWT or cookie from the auth flow)
        // Authentication is bypassed here to focus on downstream flow
        Client.DefaultRequestHeaders.Add("X-UserId", userExternalId.ToString());
        Client.DefaultRequestHeaders.Add("X-UserPerm", 
            string.Join(",", new[] { PermissionNames.CollectionWrite, PermissionNames.CollectionRead, PermissionNames.CollectionDelete }));
        
        var addCollectionResponse = await Client.PostAsJsonAsync
        ("/api/v1/accounts/me/collections",
            addCollectionDto,
            CancellationToken.None
        );
        
        addCollectionResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        addCollectionResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var created = await addCollectionResponse.Content.ReadFromJsonAsync<AddCollectionResponseDto>();
        
        var collectionExternalId =  created?.ExternalId ?? throw new InvalidOperationException("created MUST have a value");
        
        // Assert - create
        created.ExternalId.Should().NotBeEmpty();
        created.Description.Should().Be(addCollectionDto.Description);
        created.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        
        // Act - read
        var readCollectionResponse = await  Client.GetAsync
        ($"/api/v1/accounts/me/collections?startDate={addCollectionDto.StartDate}&endDate={addCollectionDto.EndDate}",
            CancellationToken.None
        );
        
        readCollectionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var readCollectionResponseContent = await readCollectionResponse.Content.ReadFromJsonAsync<List<GetCollectionResponseDto>>();
        
        // Assert - read
        readCollectionResponseContent.Should().NotBeEmpty();
        
        // Extract the value and assert that the item is the correct one
        var result = readCollectionResponseContent
            .Should()
            .ContainSingle(c => c.CollectionExternalId == collectionExternalId)
            .Which;
        
        // Assert the content of the item 
        result.Should().BeEquivalentTo(addCollectionDto, options => options
            .Including(x => x.Description)
            .Including(x => x.EstimatedBudget)
            .Including(x => x.RealBudget)
        );
        
        // Assert remaining fields 
        result.StartDate.Should().BeCloseTo(addCollectionDto.StartDate, TimeSpan.FromSeconds(10));
        result.EndDate.Should().BeCloseTo(addCollectionDto.EndDate, TimeSpan.FromSeconds(10));
        
        // Arrange - update
        var updateCollectionDto = new TransactionCollectionBuilder()
            .BuildUpdateCollectionRequestDto(collectionExternalId.ToString());
        
        // Act - update
        var updateCollectionResponse = await Client.PutAsJsonAsync(
            "/api/v1/accounts/me/collections",
            updateCollectionDto,
            CancellationToken.None
        );
        
        updateCollectionResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var collectionsAfterUpdateResponse = await Client.GetAsync(
            "/api/v1/accounts/me/collections",
            CancellationToken.None
        );
        
        collectionsAfterUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var collectionsAfterUpdate = await  collectionsAfterUpdateResponse.Content.ReadFromJsonAsync<List<GetCollectionResponseDto>>();
        
        // Assert - update
        collectionsAfterUpdate.Should().NotBeEmpty();
        
        // Extract the value and assert that the item is the correct one
        var collectionAfterUpdate = collectionsAfterUpdate
            .Should()
            .ContainSingle(r => r.CollectionExternalId == collectionExternalId)
            .Which;

        // Assert the content of the item 
        collectionAfterUpdate
            .Should()
            .BeEquivalentTo(updateCollectionDto, options => options
                .Including(x => x.Description)
                .Including(x => x.EstimatedBudget)
                .Including(x => x.RealBudget)
            );
        
        // Assert remaining fields 
        collectionAfterUpdate.StartDate.Should().BeCloseTo((DateTimeOffset)updateCollectionDto.StartDate!, TimeSpan.FromSeconds(10));
        collectionAfterUpdate.EndDate.Should().BeCloseTo((DateTimeOffset)updateCollectionDto.EndDate!, TimeSpan.FromSeconds(10));
        
        // Act - delete
        var deleteCollectionResponse = await Client.DeleteAsync(
            $"/api/v1/accounts/me/collections/{collectionExternalId}",
            CancellationToken.None
        );
        
        deleteCollectionResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var collectionsAfterDeleteResponse = await Client.GetAsync(
            "/api/v1/accounts/me/collections",
            CancellationToken.None
        );
        
        collectionsAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var collectionsAfterDelete = await   collectionsAfterDeleteResponse.Content.ReadFromJsonAsync<List<GetCollectionResponseDto>>();
        
        // Assert - delete
        collectionsAfterDelete.Should().BeEmpty();
    }
    
    private async Task <Guid> SeedUserData()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new UserBuilder()
            .Build();

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        return user.ExternalId;
    }
}