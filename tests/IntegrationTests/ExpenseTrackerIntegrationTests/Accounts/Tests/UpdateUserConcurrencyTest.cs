using System.Net;
using System.Net.Http.Json;
using ErrorOr;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Application.Accounts.Errors;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Infrastructure.Database;
using ExpenseTracker.IntegrationTests.Accounts.Builder;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.IntegrationTests.Accounts.Tests;

public class UpdateUserConcurrencyTest : BaseIntegrationTest
{
    public UpdateUserConcurrencyTest(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateUserOnConcurrentRequest_ShouldFail()
    {
        // Arrange
        var (userExternalId, firstUpdateDto, secondUpdateDto) = await SeedUserData();
        
        //Simulate authentication by adding required headers (in a real scenario, we would obtain a JWT or cookie from the auth flow)
        // Authentication is bypassed here to focus on downstream flow
        Client.DefaultRequestHeaders.Add("X-UserId", userExternalId.ToString());
        Client.DefaultRequestHeaders.Add("X-UserPerm", 
            string.Join(",", new[] { PermissionNames.UserWrite }));
        
        // Act 
        var firstTask = Task.Run(async () =>
        {
            return await SendRequest(firstUpdateDto);
        });
        
        var secondTask = Task.Run(async () =>
        {
            return await SendRequest(secondUpdateDto);
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

    private async Task<(string, UpdateUserRequestDto, UpdateUserRequestDto)> SeedUserData()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var userSeed = new UserBuilder()
            .Build();
        
        await db.Users.AddAsync(userSeed);
        await db.SaveChangesAsync();
        
        var firstUpdateDto = new UserBuilder()
            .BuildUpdateUserDto(userSeed.ExternalId.ToString());
        
        var secondUpdateDto = new UserBuilder()
            .BuildUpdateUserDto(userSeed.ExternalId.ToString());

        return (userSeed.ExternalId.ToString(), firstUpdateDto, secondUpdateDto);
    }

    private async Task<HttpResponseMessage> SendRequest(UpdateUserRequestDto request)
    {
        return await Client.PutAsJsonAsync(
            $"/api/v1/accounts",
            request,
            CancellationToken.None
        );
    }
}