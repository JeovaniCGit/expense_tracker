using ExpenseTracker.Domain.Authorization.Perms.Entity;
using ExpenseTracker.Domain.Authorization.Tokens.Entity;
using ExpenseTracker.Domain.Authorization.UserRoles.Entity;
using ExpenseTracker.Domain.Categories.Entity;
using ExpenseTracker.Domain.Records.Entity;
using ExpenseTracker.Domain.Accounts.Entity;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Domain.Collections.Entity;
using ExpenseTracker.Domain.Email.Entity;

namespace ExpenseTracker.Infrastructure.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    public DbSet<PasswordHistory> PasswordHistory { get; set; }

    public DbSet<TransactionRecord> TransactionRecords { get; set; }

    public DbSet<TransactionRecordCategory> TransactionRecordCategories { get; set; }

    public DbSet<Token> Tokens { get; set; }

    public DbSet<EmailDelivery> EmailDeliveries { get; set; } 

    public DbSet<TokenType> TokenTypes { get; set; }

    public DbSet<UserRole> UserRoles { get; set; }

    public DbSet<Permission> Permissions { get; set; }

    public DbSet<TransactionCollection> Collections {  get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
