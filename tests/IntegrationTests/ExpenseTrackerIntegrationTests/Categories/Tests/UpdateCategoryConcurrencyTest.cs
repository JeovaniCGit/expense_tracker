using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Categories.Contracts.Requests;
using ExpenseTracker.Infrastructure.Database;
using ExpenseTracker.IntegrationTests.Accounts.Builder;
using ExpenseTracker.IntegrationTests.Categories.Builder;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.IntegrationTests.Categories.Tests;

public class UpdateCategoryConcurrencyTest : BaseIntegrationTest
{
    public UpdateCategoryConcurrencyTest(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task UpdateRecordOnConcurrentWriteOperation_ShouldFail()
    {
        var categoryId= await SeedCategoryData();
        
        using var firstScope = Factory.Services.CreateScope();
        var firstDbContext = firstScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        using var secondScope = Factory.Services.CreateScope();
        var secondDbContext = secondScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var categoryOriginalRead = await firstDbContext.TransactionRecordCategories.FindAsync(categoryId);
        var categorySecondRead = await secondDbContext.TransactionRecordCategories.FindAsync(categoryId);

        categoryOriginalRead.CategoryName = "Car";
        await firstDbContext.SaveChangesAsync();

        categorySecondRead.CategoryName = "Insurance";
        
        await FluentActions.Awaiting(() => secondDbContext.SaveChangesAsync())
            .Should()
            .ThrowAsync<DbUpdateConcurrencyException>();
        
        var firstWriteResult = await firstDbContext.TransactionRecordCategories.FindAsync(categoryId);
        firstWriteResult.CategoryName.Should().Be(categoryOriginalRead.CategoryName);
    }

    private async Task<long> SeedCategoryData()
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
        
        return categorySeed.Id;
    }
}