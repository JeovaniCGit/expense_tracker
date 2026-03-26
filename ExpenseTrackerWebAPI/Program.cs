using ExpenseTracker.API;
using ExpenseTracker.API.Swagger;
using ExpenseTracker.Application;
using ExpenseTracker.Infrastructure;
using Hangfire;
using Serilog;

namespace ExpenseTrackerWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotNetEnv.Env.Load();
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseLoggingConfiguration();

            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen(x =>
            {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                x.IncludeXmlComments(xmlPath);
            });
            builder.Services.AddRequestTimeout();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication(builder.Configuration);
            builder.Services.AddApiSetup(builder.Configuration);

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(x =>
                {
                    foreach (var description in app.DescribeApiVersions())
                    {
                        x.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName);
                    }
                });
            }

            app.UseHttpsRedirection();
            app.AddCustomMiddleware();
            app.UseSerilogRequestLogging();
            app.UseCors("Default");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();
            app.UseHangfireDashboard();
            app.MapControllers();

            // Creates a DI scope to safely add the existing jobs to the hangfire storage on startup
            using var scope = app.Services.CreateScope();
            var scheduler = scope.ServiceProvider.GetRequiredService<RecurringJobsScheduler>();
            scheduler.AddRecurringJobs();

            app.Run();
        }
    }
}
