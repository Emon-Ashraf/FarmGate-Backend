using System.Security.Claims;
using DAL.Data;
using DAL.Enums;
using FarmGate_Backend.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace FarmGate_Backend.Controllers;

[ApiController]
[Route("api/images")]
public sealed class ImagesController(FileImageStorage storage, ApplicationDbContext db, ILogger<ImagesController> logger) : ControllerBase
{
    [HttpPost("{purpose}")]
    [Authorize(Roles = "Farmer,Customer")]
    [EnableRateLimiting("images")]
    [RequestSizeLimit(FileImageStorage.MaxBytes + 65536)]
    [RequestFormLimits(MultipartBodyLengthLimit = FileImageStorage.MaxBytes)]
    public async Task<IActionResult> Upload(string purpose, IFormFile file, CancellationToken ct)
    {
        if (purpose is not ("product" or "avatar")) return Problem(statusCode: 400, title: "Unknown image purpose.");
        if (!long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return Unauthorized();
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
        if (user == null || (purpose == "product" && user.Role != RoleType.Farmer)) return Forbid();
        if (!storage.Enabled) return Problem(statusCode: 503, title: "Image storage is not configured.");
        if (file.Length == 0 || file.Length > FileImageStorage.MaxBytes) return Problem(statusCode: 400, title: "Image must be between 1 byte and 5 MB.");
        try
        {
            await using var stream = file.OpenReadStream();
            return Ok(await storage.SaveAsync(stream, file.ContentType, id, purpose, ct));
        }
        catch (BLL.Exceptions.BadRequestException ex) { return Problem(statusCode: 400, title: ex.Message); }
        catch (IOException ex) { logger.LogError(ex, "Image storage failed"); return Problem(statusCode: 503, title: "Image storage is temporarily unavailable. Try again."); }
    }

    [HttpGet("{name}")]
    [AllowAnonymous]
    public IActionResult Read(string name)
    {
        var path = storage.GetPath(name);
        if (path == null || !System.IO.File.Exists(path)) return NotFound();
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers.CacheControl = "public,max-age=86400,immutable";
        return PhysicalFile(path, "image/jpeg", enableRangeProcessing: true);
    }
}
