using Microsoft.AspNetCore.Mvc.Testing;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
    {
    protected readonly HttpClient Client;
    protected readonly IntegrationTestWebAppFactory Factory;

    // The constructor takes an instance of the IntegrationTestWebAppFactory, which is provided by the IClassFixture interface. This factory is used to create an HttpClient that will be used to send requests to the test server. 
    // The factory also provides access to the WireMock server for stubbing external API calls.
    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient( new WebApplicationFactoryClientOptions
        {
            HandleCookies = true // Enable cookie handling for authentication tests
        });
    }

    protected void StubSendGridSuccess()
    {
        Factory.WireMockServer
            .Given(Request.Create().WithPath("/v3/mail/send").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(202));
    }

    protected void StubSendGridFailure()
    {
        Factory.WireMockServer
            .Given(Request.Create().WithPath("/v3/mail/send").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(400));
    }
}