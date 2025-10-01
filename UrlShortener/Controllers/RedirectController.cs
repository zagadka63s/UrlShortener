using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Controllers;

[ApiController]
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

        // 302 temporary redirect 
        return Redirect(target);
    }
}
