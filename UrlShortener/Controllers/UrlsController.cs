using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

using UrlShortener.Application.Abstractions;
using UrlShortener.Application.DTO;
using UrlShortener.Infrastructure.Persistence;
using UrlShortener.RealTime;

namespace UrlShortener.Controllers;

/// <summary>
/// controller to manage short URLs: list, create, delete
/// </summary>
[ApiController]
[Route("api/urls")]
public class UrlsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<UrlsHub> _hub;

    public UrlsController(AppDbContext db, IHubContext<UrlsHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    /// <summary>
    /// Public list of short links with search/pagination.
    /// GET /api/urls?q=term&page=1&pageSize=20
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResult<ShortUrlDto>>> GetAll(
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 100) pageSize = 100;

        var query = _db.ShortUrls.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x =>
                x.OriginalUrl.Contains(term) ||
                x.ShortCode.Contains(term));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ShortUrlDto
            {
                Id = x.Id,
                OriginalUrl = x.OriginalUrl,
                ShortCode = x.ShortCode,
                CreatedAt = x.CreatedAt,
                CreatedByUserId = x.CreatedByUserId
            })
            .ToListAsync(ct);

        return Ok(new PagedResult<ShortUrlDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        });
    }

    /// <summary>
    /// Record details by ID (for Info view). For authorized users only.
    /// </summary>
    [Authorize]
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ShortUrlDto>> GetById(long id, CancellationToken ct)
    {
        var dto = await _db.ShortUrls
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new ShortUrlDto
            {
                Id = s.Id,
                OriginalUrl = s.OriginalUrl,
                ShortCode = s.ShortCode,
                CreatedAt = s.CreatedAt,
                CreatedByUserId = s.CreatedByUserId
            })
            .FirstOrDefaultAsync(ct);

        if (dto is null) return NotFound();
        return Ok(dto);
    }

    /// <summary>
    /// Creating a short link. Authorized users only.
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ShortUrlDto>> Create(
        [FromBody] CreateShortUrlRequest request,
        [FromServices] IUrlNormalizer normalizer,
        [FromServices] IShortCodeGenerator codeGen,
        CancellationToken ct)
    {
        // --- validation ---
        if (request is null) return BadRequest("Request body is required.");
        if (string.IsNullOrWhiteSpace(request.OriginalUrl))
            return BadRequest("OriginalUrl is required");

        var validation = ValidateOriginalUrl(request.OriginalUrl);
        if (!validation.ok) return BadRequest(validation.message);

        if (request.OriginalUrl.Length > 2048)
            return BadRequest("URL is too long (max 2048 characters).");

        string normalizedUrl;
        try
        {
            normalizedUrl = normalizer.Normalize(request.OriginalUrl);
        }
        catch (UriFormatException ex)
        {
            return BadRequest(ex.Message);
        }

        // --- duplicates ---
        var exists = await _db.ShortUrls.AsNoTracking()
            .AnyAsync(x => x.NormalizedOriginalUrl == normalizedUrl, ct);
        if (exists) return Conflict(new { message = "URL already exists." });

        // --- code generation ---
        string code;
        do
        {
            code = codeGen.Next();
        } while (await _db.ShortUrls.AnyAsync(x => x.ShortCode == code, ct));

        // --- owner ---
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var entity = new UrlShortener.Domain.Entities.ShortUrl
        {
            OriginalUrl = request.OriginalUrl.Trim(),
            NormalizedOriginalUrl = normalizedUrl,
            ShortCode = code,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.ShortUrls.Add(entity);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Duplicate short url or original url." });
        }

        await _hub.Clients.All.SendAsync("urlsChanged", cancellationToken: ct);

        var dto = new ShortUrlDto
        {
            Id = entity.Id,
            OriginalUrl = entity.OriginalUrl,
            ShortCode = entity.ShortCode,
            CreatedAt = entity.CreatedAt,
            CreatedByUserId = entity.CreatedByUserId
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, dto);
    }

    /// <summary>
    /// Deletion: either the owner of the record or the administrator.
    /// </summary>
    [Authorize]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var entity = await _db.ShortUrls.FindAsync([id], ct);
        if (entity is null) return NotFound();

        var isAdmin = User.IsInRole("Admin");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!isAdmin && (string.IsNullOrEmpty(userId) || entity.CreatedByUserId != userId))
            return Forbid();

        _db.ShortUrls.Remove(entity);
        await _db.SaveChangesAsync(ct);

        await _hub.Clients.All.SendAsync("urlsChanged", cancellationToken: ct);

        return NoContent();
    }

    // --- Helpers ---
    private static (bool ok, string? message) ValidateOriginalUrl(string input)
    {
        var raw = input?.Trim();
        if (string.IsNullOrWhiteSpace(raw))
            return (false, "OriginalUrl is required");

        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri))
            return (false, "Invalid URL format");

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return (false, "Only http/https protocols are allowed");

        if (string.IsNullOrEmpty(uri.Host))
            return (false, "Host is required");

        return (true, null);
    }
}
