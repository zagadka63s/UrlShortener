using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortener.Application.Abstractions

/// <summary>
/// Generates short codes for shortened URLs.
/// </summary>
{
    public interface IShortCodeGenerator
    {
        /// <summary>
        /// Returns a new short code using an URL-safe alphabet.
        /// </summary>
        string Next(int length = 7);
    }
}
