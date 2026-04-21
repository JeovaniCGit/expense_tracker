using DotNetEnv;
using ExpenseTracker.API;
using ExpenseTracker.Infrastructure.Database;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using SendGrid;
using Testcontainers.PostgreSql;
using WireMock.Server;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime 
{
    // Using Testcontainers to manage a PostgreSQL instance for integration testing
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("expensetracker")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true)
        .WithAutoRemove(true)
        .Build();

    // WireMock server URL for stubbing SendGrid API calls
    public IntegrationTestWebAppFactory()
    {
        Env.Load("../../../.env.test"); // Load environment variables from .env.Test file
        WireMockServer = WireMockServer.Start();
        WireMockUrl = WireMockServer.Urls[0];
    }

    // WireMockServer instace
    public WireMockServer WireMockServer { get; private set; }
    public string WireMockUrl { get; }
    public TestTokenObserver TokenObserver { get; } = new();

    // The ConfigureWebHost method is overridden to configure the test services, including replacing the ISendGridClient with a mock that points to the WireMock server
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        Environment.SetEnvironmentVariable("ENABLE_HANGFIRE", "false");

        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContextOptions registration and replace it with one that points to the Testcontainers PostgreSQL instance
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
            });

            // Remove existing ISendGridClient registration and replace it with a mock that points to the WireMock server
            services.RemoveAll<ISendGridClient>();

            services.AddSingleton<ISendGridClient>(_ =>
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(WireMockUrl)
                };

                return new SendGridClient(httpClient, "fake-api-key");
            });

            services.RemoveAll<IVerificationTokenObserver>();
            services.AddSingleton<IVerificationTokenObserver>(TokenObserver);

            services.RemoveAll<IAuthenticationSchemeProvider>();
            services.RemoveAll<IAuthenticationHandlerProvider>();
            services.RemoveAll<IAuthenticationService>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                options.DefaultChallengeScheme = TestAuthHandler.Scheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.Scheme, _ => { });

            services.RemoveAll<IBackgroundJobClient>();
            services.AddSingleton<IBackgroundJobClient>(_ => new Mock<IBackgroundJobClient>().Object);
        });
    }

    // The InitializeAsync and DisposeAsync methods are implemented to start and stop the Testcontainers PostgreSQL instance and the WireMock server before and after the tests run
    public async Task InitializeAsync() {
        await _postgreSqlContainer.StartAsync();
        WireMockServer = WireMockServer.Start();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }

    public async Task DisposeAsync() {
        await _postgreSqlContainer.StopAsync();
        WireMockServer.Dispose();
        await base.DisposeAsync();
    }
}