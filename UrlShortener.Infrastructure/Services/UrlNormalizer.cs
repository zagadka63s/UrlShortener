using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using UrlShortener.Application.Abstractions;

namespace UrlShortener.Infrastructure.Services

/// <summary>
/// Canonicalizes absolute URLs to reduce dublicates.
/// </summary>
{
    public class UrlNormalizer : IUrlNormalizer
    {
        public string Normalize(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            
                throw new UriFormatException("URL is empty");

            url = url.Trim();

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                throw new UriFormatException("URL must be absolute (with scheme)");

            var builder = new UriBuilder(uri)
            {
                Scheme = uri.Scheme.ToLowerInvariant(),
                Host = uri.Host.ToLowerInvariant(),
            };

            // remove default ports
            if ((builder.Scheme == "http" && builder.Port == 80) || (builder.Scheme == "https" && builder.Port == 443))
            {
                builder.Port = -1;
            }

            // remove trailing slash except for root

            var path = builder.Path;
            if (path.Length > 1 && path.EndsWith("/", StringComparison.Ordinal))
                builder.Path = path.TrimEnd('/');

            return builder.Uri.ToString();
        }
    }
}
