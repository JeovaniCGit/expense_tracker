using ExpenseTracker.Application.Accounts.Services.UserServices;
using ExpenseTracker.Application.Accounts.Validators;
using ExpenseTracker.Application.Authentication.AuthenticationServices;
using ExpenseTracker.Application.Authorization.Tokens.Services;
using ExpenseTracker.Application.Categories.Services;
using ExpenseTracker.Application.Categories.Validators;
using ExpenseTracker.Application.Collections.Services;
using ExpenseTracker.Application.Collections.Validators;
using ExpenseTracker.Application.Emails.Jobs;
using ExpenseTracker.Application.Records.Services;
using ExpenseTracker.Application.Records.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Application;

public static class ApplicationSetupConfiguration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        AddServices(services);
        AddValidators(services);
        services.AddTransient<SendEmailJob>();
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITransactionRecordService, TransactionRecordService>();
        services.AddScoped<ITransactionRecordCategoryService, TransactionRecordCategoryService>();
        services.AddScoped<ICollectionService, CollectionService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<IVerificationTokenObserver, NoopVerificationTokenObserver>();
        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<AddUserDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateUserDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<AddTransactionRecordDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateTransactionRecordDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<AddTransactionRecordCategoryDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateTransactionRecordCategoryDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<AddCollectionDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateCollectionDtoValidator>();

        return services;
    }
}
