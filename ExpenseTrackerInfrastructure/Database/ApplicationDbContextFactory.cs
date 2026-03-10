using ExpenseTracker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        DotNetEnv.Env.Load();

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseNpgsql(
            configuration.GetConnectionString("EXPENSETRACKER_CONNECTION_STRING"));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}