using ExpenseTracker.Application.Accounts.Validators;
using ExpenseTracker.Application.Categories.Validators;
using ExpenseTracker.Application.Collections.Validators;
using ExpenseTracker.Application.Emails.Jobs;
using ExpenseTracker.Application.Records.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Application;

public static class ApplicationSetupConfiguration
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddValidators(services);
        services.AddTransient<SendEmailJob>();
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
