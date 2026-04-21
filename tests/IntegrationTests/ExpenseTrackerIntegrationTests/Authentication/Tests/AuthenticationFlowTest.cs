using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Application.Authentication.Contracts.Request;
using ExpenseTracker.Application.Authentication.Contracts.Response;
using ExpenseTracker.Application.Authorization.Perms.Attributes;
using ExpenseTracker.Application.Collections.Contracts.Responses;
using ExpenseTracker.IntegrationTests.Accounts.Builder;
using FluentAssertions;

public class AuthenticationFlowTest : BaseIntegrationTest
{
    public AuthenticationFlowTest(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task RegisterLoginAndAccessRecords_ShouldSucceed()
    {
        // Arrange
        var registerDto = new UserBuilder()
            .BuildCreateUserDto();

        var loginDto = new LoginRequestDto
        {
            Email = registerDto.Email,
            Password = registerDto.Password
        };

        // Act - register
        var registerResponse = await Client.PostAsJsonAsync(
            "/api/v1/auth/register",
            registerDto,
            CancellationToken.None
        );

        // Assert - register
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var registerContent = await registerResponse.Content.ReadFromJsonAsync<AddUserResponseDto>();

        // Assert - login
        registerContent.Should().NotBeNull();
        registerContent!.ExternalId.Should().NotBeEmpty();

        // Act - verify email
        var token = ExtractVerificationToken(registerDto.Email);
        var verificationResponse = await Client.PostAsync(
            $"/api/v1/auth/verify-user?emailToken={token}",
            null,
            CancellationToken.None
        );

        // Assert - verify
        verificationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - login
        var loginResponse = await Client.PostAsJsonAsync(
            "/api/v1/auth/login",
            loginDto,
            CancellationToken.None
        );

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        // Assert - login
        loginContent.Should().NotBeNull();
        loginContent!.AccessToken.Should().NotBeNullOrEmpty();
        loginContent.RefreshToken.Should().NotBeNullOrEmpty();

        // Act - access protected endpoint

        //Simulate authentication by adding required headers (in a real scenario, we would obtain a JWT or cookie from the auth flow)
        
        Client.DefaultRequestHeaders.Add("X-UserId", registerContent.ExternalId.ToString());

        Client.DefaultRequestHeaders.Add("X-UserPerm", 
            string.Join(",", new[] { PermissionNames.CollectionRead }));

        var collectionResponse = await Client.GetAsync(
            $"/api/v1/accounts/me/collections",
            CancellationToken.None
        );

        collectionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var collectionContent = await collectionResponse.Content.ReadFromJsonAsync<List<AddCollectionResponseDto>>();

        // Assert - collections
        collectionContent.Should().NotBeNull();
        collectionContent.Should().BeEmpty();
    }

    protected string? ExtractVerificationToken(string email)
    {
        var tokenFound = Factory.TokenObserver.Tokens.Where(kv => kv.Key == email).Select(kv => kv.Value).FirstOrDefault();

        return tokenFound;
    }
}
