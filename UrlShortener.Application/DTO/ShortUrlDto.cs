namespace UrlShortener.Application.DTO;

/// <summary>
/// Read model for public listing. No internal fields.
/// </summary>
public class ShortUrlDto
{
    public long Id { get; set; }
    public string OriginalUrl { get; set; } = null!;
    public string ShortCode { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public string CreatedByUserId { get; set; } = default!;
}
