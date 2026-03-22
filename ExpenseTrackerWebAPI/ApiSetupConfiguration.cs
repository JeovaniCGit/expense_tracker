using ExpenseTracker.API.Logging.Middleware;
using ExpenseTracker.API.Validation.Middleware;
using ExpenseTracker.Application.Abstractions.RateLimitingConstants;
using ExpenseTracker.Application.Authorization.Perms.Seeds;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

namespace ExpenseTracker.API;

public static class ApiSetupConfiguration
{
    public static IServiceCollection AddApiSetup(this IServiceCollection services, IConfiguration configuration)
    {
        AddAuthorizationConfiguration(services, configuration);
        AddAuthenticationConfiguration(services, configuration);
        AddRateLimiting(services);
        AddCors(services, configuration);
        AddRequestTimeout(services);

        return services;
    }

    public static IServiceCollection AddAuthorizationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorization(options =>
        {
            List<PermissionSeed> permissions = PermissionSeeds.All.ToList();
            foreach (var perm in permissions)
            {
                string name = perm.PermissionName;
                options.AddPolicy(name, policy =>
                {
                    policy.RequireClaim("Permission", name);
                });
            }
        });

        return services;
    }

    public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(x =>
        {
            x.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT_SIGNINGKEY"]!)),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = configuration["JWT_ISSUER"],
                ValidAudience = configuration["JWT_AUDIENCE"],
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });
        return services;
    }

    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("Default", policy =>
                policy.WithOrigins(configuration["CORS_ALLOWEDORIGINS"]!)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
            );
        });
        return services;
    }

    public static IHostBuilder UseLoggingConfiguration(this IHostBuilder host)
    {
        return host.UseSerilog((context, services, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration)
                         .ReadFrom.Services(services);
        });
    }

    public static WebApplication AddCustomMiddleware(this WebApplication app)
    {
        app.UseMiddleware<RequestLogContextMiddleware>();
        app.UseMiddleware<ValidationMappingMiddleware>();
        return app;
    }

    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(rtOptions =>
        {
            rtOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rtOptions.OnRejected = async (context, token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";

                    ProblemDetailsFactory problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                    ProblemDetails problemDetails = problemDetailsFactory.CreateProblemDetails
                    (
                        context.HttpContext,
                        StatusCodes.Status429TooManyRequests,
                        "Too Many Requests",
                        detail: $"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds."
                    );

                    await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: token);
                }
            };

            rtOptions.AddPolicy(RateLimitingPolicy.AuthenticatedUsers, context =>
            {
                string? userId = context.User.FindFirstValue("Sub");

                //Add Custom Exception here 
                if (string.IsNullOrWhiteSpace(userId))
                    throw new Exception();

                return RateLimitPartition.GetSlidingWindowLimiter
                   (
                       userId,
                       _ => new SlidingWindowRateLimiterOptions
                       {
                           Window = TimeSpan.FromMinutes(1),
                           SegmentsPerWindow = 5,
                           PermitLimit = 80,
                           QueueLimit = 0,
                           AutoReplenishment = true
                       }
                   );
            });

            rtOptions.AddPolicy(RateLimitingPolicy.AnonymousUser, context =>
            {
                return RateLimitPartition.GetSlidingWindowLimiter
                (
                    "anonymous",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 5,
                        PermitLimit = 15,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }
                );
            });
        });

        return services;
    }

    public static IServiceCollection AddRequestTimeout(this IServiceCollection services)
    {
        services.AddRequestTimeouts(options =>
        {
            options.AddPolicy("FastOperation", new RequestTimeoutPolicy
            {
                Timeout = TimeSpan.FromSeconds(2)
            });

            options.AddPolicy("ExternalCall", new RequestTimeoutPolicy
            {
                Timeout = TimeSpan.FromSeconds(8)
            });
        });

        return services;
    }
}
