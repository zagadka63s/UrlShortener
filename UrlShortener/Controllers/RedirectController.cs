using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Controllers;

/// <summary>
/// controller to handle redirection from short URL to original URL
/// </summary>
[Route("r")]
public class RedirectController : ControllerBase
{
    private readonly AppDbContext _db;

    public RedirectController(AppDbContext db) => _db = db;

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var target = await _db.ShortUrls
            .AsNoTracking()
            .Where(x => x.ShortCode == code)
            .Select(x => x.OriginalUrl)
            .FirstOrDefaultAsync(ct);

        if (target is null) return NotFound();

        
        return Redirect(target);
    }
}
