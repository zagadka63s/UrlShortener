using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortener.Application.DTO

/// <summary>
/// Request payload for creating a short URL.
/// </summary>
{
    public class CreateShortUrlRequest
    {
        public string OriginalUrl { get; set; } = null!;
    }
}
