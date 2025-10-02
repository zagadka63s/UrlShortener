using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortener.Application.DTO
/// <summary>
/// DTO for the "About" page content.
/// </summary>
{
    public class AboutDto
    {
        public string Content { get; set; } = string .Empty;
        public DateTime UpdatedAt { get; set; }
    }
}
