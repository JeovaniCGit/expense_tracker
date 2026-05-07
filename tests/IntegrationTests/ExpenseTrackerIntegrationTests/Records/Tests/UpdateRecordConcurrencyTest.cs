using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Records.Contracts.Requests;
using ExpenseTracker.Infrastructure.Database;
using ExpenseTracker.IntegrationTests.Accounts.Builder;
using ExpenseTracker.IntegrationTests.Categories.Builder;
using ExpenseTracker.IntegrationTests.Collections.Builder;
using ExpenseTracker.IntegrationTests.Records.Builder;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.IntegrationTests.Records.Tests;

public class UpdateRecordConcurrencyTest : BaseIntegrationTest
{
    public UpdateRecordConcurrencyTest(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task UpdateRecordOnConcurrentRequest_ShouldFail()
    {
        // Arrange
        var (userExternalId, recordExternalId, categoryExternalId) = await SeedRecordAndUserData();
        
        var updateDto = new TransactionRecordBuilder()
            .BuildUpdateTransactionRecordRequestDto(categoryExternalId, recordExternalId);
        
        //Simulate authentication by adding required headers (in a real scenario, we would obtain a JWT or cookie from the auth flow)
        // Authentication is bypassed here to focus on downstream flow
        Client.DefaultRequestHeaders.Add("X-UserId", userExternalId.ToString());
        Client.DefaultRequestHeaders.Add("X-UserPerm", 
            string.Join(",", new[] { PermissionNames.RecordWrite }));
        
        // Act
        var firstTask = Task.Run(async () =>
        {
            return await SendRequest(updateDto);
        });
        
        var secondTask = Task.Run(async () =>
        {
            return await SendRequest(updateDto);
        });
        
        await Task.WhenAll(firstTask, secondTask);

        var updateResponses = new HttpResponseMessage[]
        {
            await firstTask,
            await secondTask
        };
        
        // Assert
        updateResponses.Count(r => r.StatusCode == HttpStatusCode.NoContent)
            .Should().Be(1);
        
        updateResponses.Count(r => r.StatusCode == HttpStatusCode.Conflict)
            .Should().Be(1);
    }
    
    private async Task<(string, string, string)> SeedRecordAndUserData()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var userSeed = new UserBuilder()
            .Build();

        await db.Users.AddAsync(userSeed);
        await db.SaveChangesAsync();
        
        var (categorySeedId, collectionSeedId, categoryExternalId) = await SeedCollectionAndCategoryData(userSeed.Id, db);
        
        var recordSeed = new TransactionRecordBuilder()
            .WithTransactionUserId(userSeed.Id)
            .WithTransactionCategoryId(categorySeedId)
            .WithTransactionCollectionId(collectionSeedId)
            .Build();
        
        await db.TransactionRecords.AddAsync(recordSeed);
        await db.SaveChangesAsync();
        
        return (userSeed.ExternalId.ToString(), recordSeed.ExternalId.ToString(), categoryExternalId);
    }

    private async Task<(long, long, string)> SeedCollectionAndCategoryData(long userId, ApplicationDbContext context)
    {
        var categorySeed = new TransactionRecordCategoryBuilder()
            .WithUserId(userId)
            .Build();

        await context.TransactionRecordCategories.AddAsync(categorySeed);
        await context.SaveChangesAsync();
        
        var collectionSeed = new TransactionCollectionBuilder()
            .WithUserId(userId)
            .Build();
        
        await context.Collections.AddAsync(collectionSeed);
        await context.SaveChangesAsync();
        
        return (categorySeed.Id, collectionSeed.Id, categorySeed.ExternalId.ToString());
    }
    
    private async Task<HttpResponseMessage> SendRequest(UpdateTransactionRecordRequestDto request)
    {
        return await Client.PutAsJsonAsync(
            $"/api/v1/accounts/me/records/{request.TransactionExternalId}",
            request,
            CancellationToken.None
        );
    }
}