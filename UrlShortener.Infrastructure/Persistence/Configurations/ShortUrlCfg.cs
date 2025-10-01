using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Persistence.Configurations;

public class ShortUrlCfg : IEntityTypeConfiguration<ShortUrl>
{
    public void Configure(EntityTypeBuilder<ShortUrl> e)
    {
        e.HasKey(x => x.Id);

        e.Property(x => x.OriginalUrl).IsRequired();
        e.Property(x => x.NormalizedOriginalUrl).IsRequired();
        e.Property(x => x.ShortCode).HasMaxLength(12).IsRequired();
        e.Property(x => x.CreatedByUserId).IsRequired();

        e.HasIndex(x => x.ShortCode).IsUnique();
        e.HasIndex(x => x.NormalizedOriginalUrl).IsUnique();

        e.Property(x => x.CreatedAt).HasConversion(
            v => v,               // уже в UTC
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
    }
}
