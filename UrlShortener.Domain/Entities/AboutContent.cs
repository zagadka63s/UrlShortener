using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortener.Domain.Entities;

/// <summary>
/// Text of the “About” section. Visible to everyone, editable only by the administrator.
/// </summary>
public class AboutContent
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty; 
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedByUserId { get; set; }
}

