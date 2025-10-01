using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.DTO;
using UrlShortener.Infrastructure.Persistence;
using UrlShortener.Application.Abstractions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace UrlShortener.Controllers;

/// <summary>
/// Public read endpoints for short URLs. Creation/deletion will be added later with auth.
/// </summary>
[ApiController]
[Route("api/urls")]
public class UrlsController : ControllerBase
{
    private readonly AppDbContext _db;
    public UrlsController(AppDbContext db) => _db = db;

    /// <summary>
    /// Returns a public list of short URLs. Pagination can be added later.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShortUrlDto>>> GetAll(CancellationToken ct)
    {
        var items = await _db.ShortUrls
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ShortUrlDto
            {
                Id = x.Id,
                OriginalUrl = x.OriginalUrl,
                ShortCode = x.ShortCode,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(items);
    }

    ///<summary>
    /// Creates a short URL for the given original URL.
    /// (Auth will be added later.)
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ShortUrlDto>> Create(
        [FromBody] CreateShortUrlRequest request,
        [FromServices] IUrlNormalizer normalizer,
        [FromServices] IShortCodeGenerator codeGen,
        CancellationToken ct)

    {
        if (request is null || string.IsNullOrWhiteSpace(request.OriginalUrl))
            return BadRequest("OriginalUrl is required");

        string normalizedUrl;
        try
        {
            normalizedUrl = normalizer.Normalize(request.OriginalUrl);
        }

        catch (UriFormatException ex)
        {
            return BadRequest(ex.Message);
        }

        var exists = await _db.ShortUrls.AsNoTracking()
            .AnyAsync(x => x.NormalizedOriginalUrl == normalizedUrl, ct);
        if (exists) return Conflict(new { message = "URL already exists." });
        string code;
        do
        {
            code = codeGen.Next();
        } while (await _db.ShortUrls.AnyAsync(x => x.ShortCode == code, ct));

        // TODO : when auth is added, use the real user id from claims
        var entity = new UrlShortener.Domain.Entities.ShortUrl
        {
            OriginalUrl = request.OriginalUrl.Trim(),
            NormalizedOriginalUrl = normalizedUrl,
            ShortCode = code,
            CreatedByUserId = "anonymous",
            CreatedAt = DateTime.UtcNow
        };

        _db.ShortUrls.Add(entity);
        await _db.SaveChangesAsync(ct);

        var dto = new ShortUrlDto
        {
            Id = entity.Id,
            OriginalUrl = entity.OriginalUrl,
            ShortCode = entity.ShortCode,
            CreatedAt = entity.CreatedAt
        };

        return Created($"/r/{dto.ShortCode}", dto);
    }

    ///<summary>
    ///Deletes a short URL by id. AUTH will be added later.
    /// </summary>
    [Authorize]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var entity = await _db.ShortUrls.FindAsync([id], ct);
        if (entity is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (!(isAdmin || (!string.IsNullOrEmpty(userId) && entity.CreatedByUserId == userId)))
            return Forbid();

        _db.ShortUrls.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

}
