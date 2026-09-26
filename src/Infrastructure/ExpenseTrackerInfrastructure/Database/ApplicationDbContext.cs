using ExpenseTracker.Domain.Authorization.Tokens.Entity;
using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Domain.Records.Entity;
using ExpenseTracker.Domain.Accounts.Entity;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Domain.Collections.Entity;

namespace ExpenseTracker.Infrastructure.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<TransactionRecord> TransactionRecords { get; set; }
    public DbSet<TransactionRecordCategory> TransactionRecordCategories { get; set; }
    public DbSet<Token> Tokens { get; set; }

    public DbSet<TokenType> TokenTypes { get; set; }
    public DbSet<TransactionCollection> Collections {  get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
