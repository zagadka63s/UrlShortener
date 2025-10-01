using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortener.Domain.Entities;

/// <summary>
/// Shortened URL entry. Only domain data is included here.
/// </summary>
public class ShortUrl
{
    public long Id { get; set; }
    public string OriginalUrl { get; set; } = null!;
    public string NormalizedOriginalUrl { get; set; } = null!;
    public string ShortCode { get; set; } = null!;
    public string CreatedByUserId { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
