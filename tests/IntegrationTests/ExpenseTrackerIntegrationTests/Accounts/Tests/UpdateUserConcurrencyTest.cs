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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.IntegrationTests.Accounts.Tests;

public class UpdateUserConcurrencyTest : BaseIntegrationTest
{
    public UpdateUserConcurrencyTest(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task UpdateRecordOnConcurrentWriteOperation_ShouldFail()
    {
        var userId= await SeedUserData();
        
        using var firstScope = Factory.Services.CreateScope();
        var firstDbContext = firstScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        using var secondScope = Factory.Services.CreateScope();
        var secondDbContext = secondScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var userOriginalRead = await firstDbContext.Users.FindAsync(userId);
        var userSecondRead = await secondDbContext.Users.FindAsync(userId);

        userOriginalRead.Firstname = "Brian";
        await firstDbContext.SaveChangesAsync();

        userSecondRead.Firstname = "Ron";
        
        await FluentActions.Awaiting(() => secondDbContext.SaveChangesAsync())
            .Should()
            .ThrowAsync<DbUpdateConcurrencyException>();
        
        var firstWriteResult = await firstDbContext.Users.FindAsync(userId);
        firstWriteResult.Firstname.Should().Be(userOriginalRead.Firstname);
    }
    
    private async Task<long> SeedUserData()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var userSeed = new UserBuilder()
            .Build();
        
        await db.Users.AddAsync(userSeed);
        await db.SaveChangesAsync();
        
        var firstUpdateDto = new UserBuilder()
            .BuildUpdateUserDto(userSeed.Version);
        
        var secondUpdateDto = new UserBuilder()
            .BuildUpdateUserDto(userSeed.Version);

        return userSeed.Id;
    }
}