using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Records.Contracts.Requests;
using ExpenseTracker.Application.Records.Contracts.Responses;
using ExpenseTracker.Infrastructure.Database;
using ExpenseTracker.IntegrationTests.Accounts.Builder;
using ExpenseTracker.IntegrationTests.Categories.Builder;
using ExpenseTracker.IntegrationTests.Collections.Builder;
using ExpenseTracker.IntegrationTests.Records.Builder;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

public class RecordsCrudFlowTest : BaseIntegrationTest
{
    public RecordsCrudFlowTest(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateReadUpdateDeleteRecord_ShouldSucceed()
    {
        // Arrange - seed required data
        var (userExternalId, userId, collectionExternalId, categoryExternalId) = await SeedUserCollectionCategory();

        var createRecordDto = new TransactionRecordBuilder()
            .BuildAddTransactionRecordRequestDto(userExternalId.ToString(), categoryExternalId.ToString(), collectionExternalId.ToString());

        // Act - create
        
        // Simulate authentication by adding required headers (in a real scenario, we would obtain a JWT or cookie from the auth flow)
        // Authentication is bypassed here to focus on downstream flow
        Client.DefaultRequestHeaders.Add("X-UserId", userExternalId.ToString());
        Client.DefaultRequestHeaders.Add("X-UserPerm", 
            string.Join(",", new[] { PermissionNames.RecordWrite, PermissionNames.RecordRead, PermissionNames.RecordDelete }));

        var addRecordResponse = await Client.PostAsJsonAsync(
            $"/api/v1/accounts/me/records",
            createRecordDto,
            CancellationToken.None
        );

        addRecordResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        addRecordResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        var created = await addRecordResponse.Content.ReadFromJsonAsync<AddTransactionRecordResponseDto>();
        var recordExternalId = created?.ExternalId ?? throw new InvalidOperationException("created MUST have a value");

        // Assert - create
        created.Should().NotBeNull();
        created!.ExternalId.Should().NotBeEmpty();
        created.TransactionValue.Should().Be(createRecordDto.TransactionValue);

        // Act - read by category
        var readRecordResponse = await Client.GetAsync(
            $"/api/v1/accounts/me/records/by-category?categoryExternalId={categoryExternalId}",
            CancellationToken.None
        );

        readRecordResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var recordsAfterCreate = await readRecordResponse.Content.ReadFromJsonAsync<List<GetTransactionRecordResponseDto>>();

        // Assert - read
        recordsAfterCreate.Should().NotBeEmpty();
        
        // Extract the value and assert that the item is the correct one
        var recordAfterRead = recordsAfterCreate
            .Should()
            .ContainSingle(r => r.TransactionExternalId == recordExternalId)
            .Which;
    
        // Map the values to the correct type
        var expected = new
        {
            TransactionCategoryExternalId = Guid.Parse(createRecordDto.TransactionCategoryExternalId),
            TransactionValue = recordAfterRead.TransactionValue,
            TransactionCategoryName = recordAfterRead.TransactionCategoryName
        };
        
        // Assert the content of the item 
        recordAfterRead.Should().BeEquivalentTo(expected, options => options
            .Including(x => x.TransactionCategoryExternalId)
            .Including(x => x.TransactionValue)
            .Including(x => x.TransactionCategoryName)
        );

        // Arrange - update
        var updateRecordDto = new TransactionRecordBuilder()
            .BuildUpdateTransactionRecordRequestDto(categoryExternalId.ToString(), recordExternalId.ToString());
        
        // Act - update
        var updateRecordResponse = await Client.PutAsJsonAsync(
            $"/api/v1/accounts/me/records/{recordExternalId}",
            updateRecordDto,
            CancellationToken.None
        );

        updateRecordResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var recordsAfterUpdateResponse = await Client.GetAsync(
            $"/api/v1/accounts/me/records/by-category?categoryExternalId={categoryExternalId}",
            CancellationToken.None
        );

        recordsAfterUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var recordsAfterUpdate = await recordsAfterUpdateResponse.Content.ReadFromJsonAsync<List<GetTransactionRecordResponseDto>>();

        // Assert - update
        recordsAfterUpdate.Should().NotBeEmpty();
        
        // Extract the value and assert that the item is the correct one
        var recordAfterUpdate = recordsAfterUpdate
            .Should()
            .ContainSingle(r => r.TransactionExternalId == recordExternalId)
            .Which;
        
        // Assert the content of the item 
        recordAfterUpdate.Should().BeEquivalentTo(updateRecordDto, options => options
            .Including(x => x.TransactionValue)
        );

        // Act - delete
        var deleteRecordResponse = await Client.DeleteAsync(
            $"/api/v1/accounts/me/records/{recordExternalId}",
            CancellationToken.None
        );

        deleteRecordResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var recordsAfterDeleteResponse = await Client.GetAsync(
            $"/api/v1/accounts/me/records/by-category?categoryExternalId={categoryExternalId}",
            CancellationToken.None
        );

        recordsAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var recordsAfterDelete = await recordsAfterDeleteResponse.Content.ReadFromJsonAsync<List<GetTransactionRecordResponseDto>>();

        // Assert - delete
        recordsAfterDelete.Should().BeEmpty();
    }

    private async Task<(Guid UserExternalId, long UserId, Guid CollectionExternalId, Guid CategoryExternalId)> SeedUserCollectionCategory()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new UserBuilder()
            .Build();

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        var collection = new TransactionCollectionBuilder()
            .WithUserId(user.Id)
            .Build();

        await db.Collections.AddAsync(collection);
        await db.SaveChangesAsync();

        var category = new TransactionRecordCategoryBuilder()
            .WithUserId(user.Id)
            .Build();

        await db.TransactionRecordCategories.AddAsync(category);
        await db.SaveChangesAsync();

        return (user.ExternalId, user.Id, collection.ExternalId, category.ExternalId);
    }
}
