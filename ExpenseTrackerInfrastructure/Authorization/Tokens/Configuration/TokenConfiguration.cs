using ExpenseTracker.Domain.Authorization.Tokens.Entity;
using ExpenseTracker.Infrastructure.Base.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Authorization.Tokens.Configuration;

internal sealed class TokenConfiguration : BaseEntityConfiguration<Token>
{
    public override void Configure(EntityTypeBuilder<Token> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(p => p.Id)
           .ValueGeneratedOnAdd();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.ExternalId)
            .IsRequired();

        builder.HasIndex(u => u.ExternalId)
           .IsUnique();

        builder.HasIndex(t => t.TokenValue)
            .IsUnique();

        builder.Property(p => p.TokenValue)
            .IsRequired();

        builder.Property(p => p.TokenUserId)
           .IsRequired();

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.TokenUserId);

        builder.Property(p => p.TokenTypeId)
           .IsRequired();

        builder.HasOne(t => t.TokenType)
            .WithMany(t => t.Tokens)
            .HasForeignKey(t => t.TokenTypeId);
        
        builder.HasQueryFilter(t => !t.User.IsDeleted && !t.IsDeleted);
    }
}
