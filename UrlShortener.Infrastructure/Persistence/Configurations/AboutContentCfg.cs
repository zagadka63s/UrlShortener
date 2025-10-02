using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for AboutContent entity
/// </summary>
public class AboutContentCfg : IEntityTypeConfiguration<AboutContent>
{
    public void Configure(EntityTypeBuilder<AboutContent> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Content).IsRequired();

        e.Property(x => x.UpdatedAt).HasConversion(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
    }
}
