using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Infrastructure.Database;
using ExpenseTracker.IntegrationTests.Accounts.Builder;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class CreatingUserTest : BaseIntegrationTest
{
    public CreatingUserTest(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task RegisterUser_ShouldReturnCreatedUserAndSendVerificationEmail()
    {
        // Arrange
        var userDto = new UserBuilder()
            .BuildCreateUserDto();

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/v1/accounts",
            userDto,
            CancellationToken.None
        );

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var raw = await response.Content.ReadAsStringAsync();

        var responseDto = JsonSerializer.Deserialize<AddUserResponseDto>(
            raw,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        responseDto.Should().NotBeNull();

        // Assert
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userInDb = await db.Users.FirstAsync(u => u.ExternalId == responseDto!.ExternalId);
        userInDb.Should().NotBeNull();

        responseDto!.Firstname.Should().Be(userDto.Firstname);
        responseDto!.Lastname.Should().Be(userDto.Lastname);
        responseDto!.ExternalId.Should().NotBeEmpty();
    }
}
