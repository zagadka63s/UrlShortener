using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortener.Application.Abstractions;

/// <summary>
/// Normalizes input URLs for uniqueness checks and storage.
/// </summary>

    public interface IUrlNormalizer
    {
        /// < summary>
        /// Returns a canonical representation of the URL.
        /// no trailing slash except for root, trimmed
        /// Throws <see cref="UriFormatException"/> if the input is not a valid absolute URL."/>
        /// </summary>
        string Normalize(string url);
    }

