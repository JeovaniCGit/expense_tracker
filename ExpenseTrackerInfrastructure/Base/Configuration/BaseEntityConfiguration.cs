using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Base.Configuration;

public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class
{
    public abstract void Configure(EntityTypeBuilder<TEntity> builder);

    protected void ConfigureBase(EntityTypeBuilder<TEntity> builder)
    {

    }
}
