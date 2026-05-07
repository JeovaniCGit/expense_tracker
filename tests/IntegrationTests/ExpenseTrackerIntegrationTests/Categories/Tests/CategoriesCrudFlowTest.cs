using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Categories.Contracts.Requests;
using ExpenseTracker.Application.Categories.Contracts.Responses;
using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Infrastructure.Database;
using ExpenseTracker.IntegrationTests.Accounts.Builder;
using ExpenseTracker.IntegrationTests.Categories.Builder;
using FluentAssertions;
using HandlebarsDotNet.Helpers.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.IntegrationTests.Categories.Tests;

public class CategoriesCrudFlowTest : BaseIntegrationTest
{
    public CategoriesCrudFlowTest(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateReadUpdateDeleteCategory_ShouldSucceed()
    {
        // Arrange - seed required data
        var userExternalId = await SeedUserData();

        var createCategoryDto = new TransactionRecordCategoryBuilder()
            .BuildAddTransactionRecordCategoryRequestDto(userExternalId.ToString());
        
        // Act - create
        
        //Simulate authentication by adding required headers (in a real scenario, we would obtain a JWT or cookie from the auth flow)
        // Authentication is bypassed here to focus on downstream flow
        Client.DefaultRequestHeaders.Add("X-UserId", userExternalId.ToString());
        Client.DefaultRequestHeaders.Add("X-UserPerm", 
            string.Join(",", new[] { PermissionNames.CategoryWrite, PermissionNames.CategoryRead, PermissionNames.CategoryDelete }));

        var addCategoryResponse = await Client.PostAsJsonAsync(
            $"/api/v1/accounts/me/records/categories",
            createCategoryDto,
            CancellationToken.None
        );

        addCategoryResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        addCategoryResponse.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var created = await addCategoryResponse.Content.ReadFromJsonAsync<AddTransactionRecordCategoryResponseDto>();
        var categoryExternalId = created?.CategoryExternalId ?? throw new InvalidOperationException("created MUST have a value");
        
        // Assert - create
        created.Should().NotBeNull();
        created.CategoryExternalId.Should().NotBeEmpty();
        created.CategoryName.Should().Be(createCategoryDto.CategoryName);
        
        // Act - read
        var readCategoryResponse = await Client.GetAsync($"/api/v1/accounts/me/records/categories",
            CancellationToken.None
        );
        
        readCategoryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var categoriesAfterCreate = await readCategoryResponse.Content.ReadFromJsonAsync<List<GetTransactionRecordCategoryResponseDto>>();
        
        // Assert - read
        categoriesAfterCreate.Should().NotBeEmpty();
        
        // Extract the value and assert that the item is the correct one
        var categoriesAfterRead = categoriesAfterCreate
            .Should()
            .ContainSingle(c => c.CategoryExternalId == categoryExternalId)
            .Which;
        
        // Assert the content of the item 
        categoriesAfterRead.Should().BeEquivalentTo(createCategoryDto, options => options
            .Including(x => x.CategoryName)
        );
        
        // Arrange - update
        var updateCategoryDto = new TransactionRecordCategoryBuilder()
            .BuildUpdateTransactionRecordCategoryRequestDto(categoryExternalId.ToString());
        
        // Act - update
        var updateCategoryResponse = await Client.PutAsJsonAsync(
            $"/api/v1/accounts/me/records/categories/{created.CategoryExternalId}",
            updateCategoryDto,
            CancellationToken.None
        );
        
        updateCategoryResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var categoriesAfterUpdateResponse = await Client.GetAsync($"/api/v1/accounts/me/records/categories",
            CancellationToken.None
        );
        
        categoriesAfterUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var categoriesAfterUpdateContent = await categoriesAfterUpdateResponse.Content.ReadFromJsonAsync<List<GetTransactionRecordCategoryResponseDto>>();
        
        // Assert - update
        categoriesAfterUpdateContent.Should().NotBeEmpty();
        
        // Extract the value and assert that the item is the correct one
        var categoriesAfterUpdate = categoriesAfterUpdateContent
            .Should()
            .ContainSingle(c => c.CategoryExternalId == categoryExternalId)
            .Which;
        
        // Assert the content of the item 
        categoriesAfterUpdate.Should().BeEquivalentTo(updateCategoryDto, options => options
            .Including(x => x.CategoryName)
        );
        
        // Act - delete
        var deleteCategoryResponse = await Client.DeleteAsync(
            $"/api/v1/accounts/me/records/categories/{created.CategoryExternalId}",
            CancellationToken.None
        );
        
        deleteCategoryResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deleteCategoryAfterResponse = await Client.GetAsync($"/api/v1/accounts/me/records/categories",
            CancellationToken.None
        );
        
        deleteCategoryAfterResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var categoriesAfterDelete = await deleteCategoryAfterResponse.Content.ReadFromJsonAsync<List<GetTransactionRecordCategoryResponseDto>>();
        
        // Assert - delete
        categoriesAfterDelete.Should().BeEmpty();
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