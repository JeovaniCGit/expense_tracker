using ExpenseTracker.API;
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
            var builder = WebApplication.CreateBuilder(args);
            DotNetEnv.Env.Load();

            builder.Host.UseLoggingConfiguration();

            builder.Services.AddControllers();
            builder.Services.AddRequestTimeout();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication(builder.Configuration);
            builder.Services.AddApiSetup(builder.Configuration);

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
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

            app.Run();
        }
    }
}
