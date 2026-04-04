using ExpenseTracker.Application.Abstractions.GuidSeeder;
using ExpenseTracker.Application.Authorization.Tokens.Enums;
using ExpenseTracker.Domain.Authorization.Tokens.Entity;
using ExpenseTracker.Infrastructure.Base.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Authorization.Tokens.Configuration;

internal sealed class TokenTypeConfiguration : BaseEntityConfiguration<TokenType>
{
    public override void Configure(EntityTypeBuilder<TokenType> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(p => p.Id)
           .ValueGeneratedOnAdd();

        builder.Property(p => p.ExternalId)
            .IsRequired();

        builder.HasIndex(u => u.ExternalId)
           .IsUnique();

        builder.Property(p => p.TokenTypeDescription)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(i => i.TokenTypeDescription)
            .IsUnique();

        var tokenTypeIds = Enum.GetValues(typeof(TokenDescriptionEnum))
            .Cast<TokenDescriptionEnum>()
            .Select(tt => (long)tt);

        var tokenTypeSeed = tokenTypeIds.Select(id => new TokenType
        {
            Id = id,
            ExternalId = GuidSeed.CreateGuidFromName(((TokenDescriptionEnum)id).ToString()),
            TokenTypeDescription = ((TokenDescriptionEnum)id).ToString(),
            TimeToLiveInMinutes = Convert.ToInt32(TimeSpan.FromDays(5).TotalMinutes)
        }).ToArray();

        builder.HasData(tokenTypeSeed);
    }
}
