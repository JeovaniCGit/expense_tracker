using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Records.Contracts.Requests;
using ExpenseTracker.Application.Records.Contracts.Responses;
using ExpenseTracker.Infrastructure.Database;
using ExpenseTracker.IntegrationTests.Accounts.Builder;
using ExpenseTracker.IntegrationTests.Categories.Builder;
using ExpenseTracker.IntegrationTests.Collections.Builder;
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

        var createRecordDto = new AddTransactionRecordRequestDto
        {
            TransactionValue = 5,
            TransactionUserExternalId = userExternalId.ToString(),
            TransactionCategoryExternalId = categoryExternalId.ToString(),
            TransactionCollectionExternalId = collectionExternalId.ToString()
        };

        // Act - create
        
        //Simulate authentication by adding required headers (in a real scenario, we would obtain a JWT or cookie from the auth flow)
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
        var recordExternalId = created?.ExternalId ?? Guid.Empty;

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
        recordsAfterCreate.Should().ContainSingle(r =>
            r.TransactionExternalId == recordExternalId &&
            r.TransactionCategoryExternalId == categoryExternalId);

        // Act - update
        var updateRecordDto = new UpdateTransactionRecordRequestDto
        {
            TransactionExternalId = recordExternalId.ToString(),
            TransactionCategoryExternalId = categoryExternalId.ToString(),
            TransactionValue = 10
        };

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
        recordsAfterUpdate.Should().ContainSingle(r =>
            r.TransactionExternalId == recordExternalId &&
            r.TransactionValue == updateRecordDto.TransactionValue);

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

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var collection = new TransactionCollectionBuilder()
            .WithUserId(user.Id)
            .Build();

        db.Collections.Add(collection);
        await db.SaveChangesAsync();

        var category = new TransactionRecordCategoryBuilder()
            .WithUserId(user.Id)
            .Build();

        db.TransactionRecordCategories.Add(category);
        await db.SaveChangesAsync();

        return (user.ExternalId, user.Id, collection.ExternalId, category.ExternalId);
    }
}
