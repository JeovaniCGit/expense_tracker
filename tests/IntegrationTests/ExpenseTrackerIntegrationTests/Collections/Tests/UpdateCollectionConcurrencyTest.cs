using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Collections.Contracts.Requests;
using ExpenseTracker.Infrastructure.Database;
using ExpenseTracker.IntegrationTests.Accounts.Builder;
using ExpenseTracker.IntegrationTests.Collections.Builder;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.IntegrationTests.Collections.Tests;

public class UpdateCollectionConcurrencyTest : BaseIntegrationTest
{
    public UpdateCollectionConcurrencyTest(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateRecordOnConcurrentWriteOperation_ShouldFail()
    {
        var collectionId= await SeedCollectionData();
        
        using var firstScope = Factory.Services.CreateScope();
        var firstDbContext = firstScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        using var secondScope = Factory.Services.CreateScope();
        var secondDbContext = secondScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var collectionOriginalRead = await firstDbContext.Collections.FindAsync(collectionId);
        var collectionSecondRead = await secondDbContext.Collections.FindAsync(collectionId);

        collectionOriginalRead.Description = "January 2023";
        await firstDbContext.SaveChangesAsync();

        collectionSecondRead.Description = "January 2025";
        
        await FluentActions.Awaiting(() => secondDbContext.SaveChangesAsync())
            .Should()
            .ThrowAsync<DbUpdateConcurrencyException>();
        
        var firstWriteResult = await firstDbContext.Collections.FindAsync(collectionId);
        firstWriteResult.Description.Should().Be(collectionOriginalRead.Description);
    }

    private async Task<long> SeedCollectionData()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var userSeed = new UserBuilder()
            .Build();

        await db.Users.AddAsync(userSeed);
        await db.SaveChangesAsync();
        
        var collectionSeed = new TransactionCollectionBuilder()
            .WithUserId(userSeed.Id)
            .Build();
        
        await db.Collections.AddAsync(collectionSeed);
        await db.SaveChangesAsync();
        
        return collectionSeed.Id;
    }
}