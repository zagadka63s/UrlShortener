using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Identity;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Infrastructure.Seed;

/// <summary>
/// Initial database seed: creates the Admin role, an admin user,
/// and a default About content record. Intended for dev/test only.
/// </summary>
public class DbSeeder
{
    private readonly AppDbContext _db;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public DbSeeder(
        AppDbContext db,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    /// <summary>
    /// Applies pending migrations and ensures baseline data exists.
    /// Idempotent: safe to call multiple times.
    /// </summary>
    public async Task SeedAsync()
    {
        // 
        await _db.Database.MigrateAsync();

        const string adminRole = "Admin";
        const string adminEmail = "admin@example.com";
        const string adminPassword = "Admin#12345"; 

        // ensure Admin role exists.
        if (!await _roleManager.RoleExistsAsync(adminRole))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(adminRole));
            if (!roleResult.Succeeded)
                throw new InvalidOperationException($"Failed to create role '{adminRole}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
        }

        // ensure admin user exists and is in the Admin role.
        var admin = await _userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
                throw new InvalidOperationException($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

            var addToRoleResult = await _userManager.AddToRoleAsync(admin, adminRole);
            if (!addToRoleResult.Succeeded)
                throw new InvalidOperationException($"Failed to add admin to role: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
        }
        else
        {
            // ensure the existing user is in the Admin role.
            if (!await _userManager.IsInRoleAsync(admin, adminRole))
            {
                var addToRoleResult = await _userManager.AddToRoleAsync(admin, adminRole);
                if (!addToRoleResult.Succeeded)
                    throw new InvalidOperationException($"Failed to add admin to role: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
            }
        }

        // seed About content if none exists
        if (!await _db.AboutContents.AnyAsync())
        {
            _db.AboutContents.Add(new AboutContent
            {
                Content = "URL Shortener: short codes are generated (e.g., base62). " +
                          "Duplicates are prevented by URL normalization and a unique index.",
                UpdatedAt = DateTime.UtcNow,
                UpdatedByUserId = admin?.Id
            });

            await _db.SaveChangesAsync();
        }
    }
}
