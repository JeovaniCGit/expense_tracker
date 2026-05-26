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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.IntegrationTests.Records.Tests;

public class UpdateRecordConcurrencyTest : BaseIntegrationTest
{
    public UpdateRecordConcurrencyTest(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateRecordOnConcurrentWriteOperation_ShouldFail()
    {
        var recordId= await SeedRecordData();
        
        using var firstScope = Factory.Services.CreateScope();
        var firstDbContext = firstScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        using var secondScope = Factory.Services.CreateScope();
        var secondDbContext = secondScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var recordOriginalRead = await firstDbContext.TransactionRecords.FindAsync(recordId);
        var recordSecondRead = await secondDbContext.TransactionRecords.FindAsync(recordId);

        recordOriginalRead.TransactionValue = 100;
        await firstDbContext.SaveChangesAsync();

        recordSecondRead.TransactionValue = 200;
        
        await FluentActions.Awaiting(() => secondDbContext.SaveChangesAsync())
            .Should()
            .ThrowAsync<DbUpdateConcurrencyException>();
        
        var firstWriteResult = await firstDbContext.TransactionRecords.FindAsync(recordId);
        firstWriteResult.TransactionValue.Should().Be(recordOriginalRead.TransactionValue);
    }
    
    private async Task<long> SeedRecordData()
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
        
        return recordSeed.Id;
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
}