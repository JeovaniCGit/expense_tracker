using DotNetEnv;
using ExpenseTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ExpenseTracker.Infrastructure;
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var root = Directory.GetCurrentDirectory();
        Env.Load(Path.Combine());

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseNpgsql(
            Environment.GetEnvironmentVariable("EXPENSETRACKER_CONNECTION_STRING"));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}