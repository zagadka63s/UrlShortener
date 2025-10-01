using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.DTO;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Controllers;

/// <summary>
///  about content endpoints : public read, admin update
/// </summary>
[ApiController]
[Route("api/about")]
public class AboutController : ControllerBase
{
    private readonly AppDbContext _db;
    public AboutController(AppDbContext db) => _db = db;

    /// <summary>
    /// Returns "About" content. Public
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<AboutDto>> Get(CancellationToken ct)
    {
        var about = await _db.AboutContents.AsNoTracking().FirstOrDefaultAsync(ct);
        if (about is null)
            return Ok(new AboutDto { Content = string.Empty, UpdatedAt = DateTime.MinValue });

        return Ok(new AboutDto { Content = about.Content, UpdatedAt = about.UpdatedAt });
    }

    public class UpdateAboutRequest
    {
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Updates "About" content. Admin only.
    /// </summary>
    /// 
    [HttpPut]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> Update([FromBody] UpdateAboutRequest req, CancellationToken ct)
    {
        if(req is null || string.IsNullOrWhiteSpace(req.Content))
            return BadRequest("Content is required");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";

        var about = await _db.AboutContents.FirstOrDefaultAsync(ct);
        if (about is null)
        {
            _db.AboutContents.Add(new Domain.Entities.AboutContent
            {
                Content = req.Content.Trim(),
                UpdatedAt = DateTime.UtcNow,
                UpdatedByUserId = userId
            });
        }
        else 
        {
            about.Content = req.Content.Trim();
            about.UpdatedAt = DateTime.UtcNow;
            about.UpdatedByUserId = userId;
        }

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

