using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ExpenseTracker.API.Swagger;

public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
{
    /// <summary>
    /// Configures Swagger generation options dynamically for each API version exposed by the application.
    ///
    /// This class implements <see cref="IConfigureNamedOptions{SwaggerGenOptions}"/> so it can be
    /// registered in the DI container and automatically configure SwaggerGen. It uses
    /// <see cref="IApiVersionDescriptionProvider"/> to discover all API versions and generates
    /// a separate Swagger document for each one. This ensures that versioned APIs appear correctly
    /// in Swagger UI with distinct endpoints for each version.
    ///
    /// Additionally, it configures JWT Bearer authentication in the OpenAPI specification, so
    /// secured endpoints are correctly marked in Swagger UI and can accept a valid token.
    ///
    /// This pattern is standard in ASP.NET Core projects that combine API versioning with
    /// Swashbuckle/Swagger, following recommendations from the ASP.NET API Versioning
    /// project wiki (Swashbuckle Integration).
    /// 
    /// see:
    ///     ref=https://github.com/dotnet/aspnet-api-versioning/wiki/Swashbuckle-Integration
    ///     ref=https://github.com/microsoft/aspnet-api-versioning/tree/master/samples/aspnetcore/SwaggerSample
    /// </summary>
    
    private readonly IApiVersionDescriptionProvider _provider;
    private readonly IHostEnvironment _environment;
    
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IHostEnvironment environment)
    {
        _provider = provider;
        _environment = environment;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = _environment.ApplicationName,
                    Version = description.ApiVersion.ToString()
                });
        }

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please provide a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }

    public void Configure(string name, SwaggerGenOptions options)
        => Configure(options);
}
