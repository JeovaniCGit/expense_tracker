using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Accounts.Services.AdminServices;
using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authorization.Tokens.Jobs;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Authorization.Tokens.Repository;
using ExpenseTracker.Domain.Categories.Repository;
using ExpenseTracker.Domain.Collection.Repository;
using ExpenseTracker.Domain.Records.Repository;
using ExpenseTracker.Infrastructure.Abstractions;
using ExpenseTracker.Infrastructure.Accounts.AnalyticsService;
using ExpenseTracker.Infrastructure.Accounts.CurrentUserService;
using ExpenseTracker.Infrastructure.Accounts.Repository;
using ExpenseTracker.Infrastructure.Authorization.Tokens.Jobs;
using ExpenseTracker.Infrastructure.Authorization.Tokens.Repository;
using ExpenseTracker.Infrastructure.Categories.Repository;
using ExpenseTracker.Infrastructure.Collections.Repository;
using ExpenseTracker.Infrastructure.Database;
using ExpenseTracker.Infrastructure.Hangfire;
using ExpenseTracker.Infrastructure.Records.Repository;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ExpenseTracker.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        AddServices(services);
        AddDatabase(services, configuration);
        AddHangfireToInfrastructure(services, configuration, environment);
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITransactionRecordRepository, TransactionRecordRepository>();
        services.AddScoped<ITransactionRecordCategoryRepository, TransactionRecordCategoryRepository>();
        services.AddScoped<ITransactionCollectionRepository, TransactionCollectionRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<IDateProvider, DateProvider>();
        services.AddScoped<IDeleteExpiredTokensService, DeleteExpiredTokensJob>();
        services.AddScoped<IAdminAnalyticsService, AdminAnalyticsService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddTransient<RecurringJobsScheduler>();
        services.AddHttpContextAccessor();

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ExpenseTracker")));

        return services;
    }

    public static IServiceCollection AddHangfireToInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsEnvironment("Test"))
        {
            return services;
        }

        services.AddHangfireServer(x =>
        {
            var hangfireOptions = configuration
                .GetSection("Hangfire")
                .Get<HangfireOptions>();

            x.SchedulePollingInterval = TimeSpan.FromSeconds(hangfireOptions!.SchedulePollingIntervalInSeconds);
            x.ServerName = hangfireOptions.ServerName;
        });

        services.AddHangfire(config =>
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(configuration.GetConnectionString("ExpenseTracker"))));

        return services;
    }
}
