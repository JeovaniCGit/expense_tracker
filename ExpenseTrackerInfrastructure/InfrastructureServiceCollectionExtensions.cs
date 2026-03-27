using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Accounts.Services.AdminServices;
using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Authentication.JwtLib;
using ExpenseTracker.Application.Authentication.JwtLib.Configuration;
using ExpenseTracker.Application.Authorization.BCryptLib;
using ExpenseTracker.Application.Authorization.Tokens.Jobs;
using ExpenseTracker.Application.Emails.Services;
using ExpenseTracker.Domain.Accounts.Repository;
using ExpenseTracker.Domain.Authorization.Tokens.Repository;
using ExpenseTracker.Domain.Categories.Repository;
using ExpenseTracker.Domain.Collection.Repository;
using ExpenseTracker.Domain.Email.Repository;
using ExpenseTracker.Domain.Records.Repository;
using ExpenseTracker.Infrastructure.Abstractions;
using ExpenseTracker.Infrastructure.Accounts.AnalyticsService;
using ExpenseTracker.Infrastructure.Accounts.CurrentUserService;
using ExpenseTracker.Infrastructure.Accounts.Repository;
using ExpenseTracker.Infrastructure.Authentication.JwtLib;
using ExpenseTracker.Infrastructure.Authentication.JwtLib.Configuration;
using ExpenseTracker.Infrastructure.Authorization.BCryptLib;
using ExpenseTracker.Infrastructure.Authorization.Tokens.Jobs;
using ExpenseTracker.Infrastructure.Authorization.Tokens.Repository;
using ExpenseTracker.Infrastructure.Categories.Repository;
using ExpenseTracker.Infrastructure.Collections.Repository;
using ExpenseTracker.Infrastructure.Database;
using ExpenseTracker.Infrastructure.Emails.Repository;
using ExpenseTracker.Infrastructure.Emails.SendGridConfiguration;
using ExpenseTracker.Infrastructure.Emails.Services;
using ExpenseTracker.Infrastructure.Hangfire;
using ExpenseTracker.Infrastructure.Records.Repository;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SendGrid;

namespace ExpenseTracker.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddServices(services);
        AddDatabase(services, configuration);
        AddHangfireToInfrastructure(services, configuration);
        AddSendGridOptions(services, configuration);
        AddJwtSigningOptions(services, configuration);
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITransactionRecordRepository, TransactionRecordRepository>();
        services.AddScoped<ITransactionRecordCategoryRepository, TransactionRecordCategoryRepository>();
        services.AddScoped<ITransactionCollectionRepository, TransactionCollectionRepository>();
        services.AddScoped<IPasswordHistoryRepository, PasswordHistoryRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IJwtTokenValidator, JwtTokenValidator>();
        services.AddScoped<IDateProvider, DateProvider>();
        services.AddScoped<IDeleteExpiredTokensService, DeleteExpiredTokensJob>();
        services.AddScoped<IAdminAnalyticsService, AdminAnalyticsService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEmailDeliveryRepository, EmailDeliveryRepository>();
        services.AddTransient<RecurringJobsScheduler>();
        services.AddHttpContextAccessor();

        services.AddSingleton<ISendGridClient>(provider =>
        {
            SendGridOptions options = provider.GetRequiredService<IOptions<SendGridOptions>>().Value;
            return new SendGridClient(options.ApiKey);
        });

        services.AddTransient<IEmailService, EmailService>();
        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("EXPENSETRACKER_CONNECTION_STRING")));

        return services;
    }

    public static IServiceCollection AddSendGridOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SendGridOptions>(options =>
        {
            options.FromEmail = configuration.GetValue<string>("FROM_EMAIL")!;
            options.FromName = configuration.GetValue<string>("FROM_NAME")!;
            options.ApiKey = configuration.GetValue<string>("SEND_GRID_API_KEY")!;
            options.VerificationTemplateId = configuration.GetValue<string>("SEND_GRID_VERIFICATION_TEMPLATE_ID")!;
            options.ResetTemplateId = configuration.GetValue<string>("SEND_GRID_RESET_TEMPLATE_ID")!;
        });
        return services;
    }

    public static IServiceCollection AddJwtSigningOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(options =>
        {
            options.SigningKey = configuration.GetValue<string>("JWT_SIGNINGKEY")!;
            options.AccessTokenExpiryMinutes = configuration.GetValue<int>("JWT_ACCESSTOKEN_EXPIRYMINUTES");
            options.RefreshTokenExpiryDays = configuration.GetValue<int>("JWT_REFRESHTOKEN_EXPIRYDAYS");
            options.Issuer = configuration.GetValue<string>("JWT_ISSUER")!;
            options.Audience = configuration.GetValue<string>("JWT_AUDIENCE")!;
        });
        return services;
    }

    public static IServiceCollection AddHangfireToInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
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
                  .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(configuration.GetConnectionString("EXPENSETRACKER_CONNECTION_STRING"))));

        return services;
    }
}
