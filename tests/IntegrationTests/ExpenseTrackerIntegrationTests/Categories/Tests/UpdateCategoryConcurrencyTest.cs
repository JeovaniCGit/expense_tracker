using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Categories.Contracts.Requests;
using ExpenseTracker.Infrastructure.Database;
using ExpenseTracker.IntegrationTests.Accounts.Builder;
using ExpenseTracker.IntegrationTests.Categories.Builder;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.IntegrationTests.Categories.Tests;

public class UpdateCategoryConcurrencyTest : BaseIntegrationTest
{
    public UpdateCategoryConcurrencyTest(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task UpdateCategoryOnConcurrentRequest_ShouldFail()
    {
        // Arrange
        var (userExternalId, categoryExternalId) = await SeedCategoryAndUserData();
        
        var updateDto = new TransactionRecordCategoryBuilder()
            .BuildUpdateTransactionRecordCategoryRequestDto(categoryExternalId);
        
        //Simulate authentication by adding required headers (in a real scenario, we would obtain a JWT or cookie from the auth flow)
        // Authentication is bypassed here to focus on downstream flow
        Client.DefaultRequestHeaders.Add("X-UserId", userExternalId.ToString());
        Client.DefaultRequestHeaders.Add("X-UserPerm", 
            string.Join(",", new[] { PermissionNames.CategoryWrite }));
        
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

        var updateResponses = new HttpResponseMessage []
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

    private async Task<(string, string)> SeedCategoryAndUserData()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var userSeed = new UserBuilder()
            .Build();
        
        await db.Users.AddAsync(userSeed);
        await db.SaveChangesAsync();
        
        var categorySeed = new TransactionRecordCategoryBuilder()
            .WithUserId(userSeed.Id)
            .Build();

        await db.TransactionRecordCategories.AddAsync(categorySeed);
        await db.SaveChangesAsync();
        
        return (userSeed.ExternalId.ToString(), categorySeed.ExternalId.ToString());
    }

    private async Task<HttpResponseMessage> SendRequest(UpdateTransactionRecordCategoryRequestDto request)
    {
        return await Client.PutAsJsonAsync(
            $"/api/v1/accounts/me/records/categories/{request.CategoryExternalId}",
            request,
            CancellationToken.None
        );
    }
}