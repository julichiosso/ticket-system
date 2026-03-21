using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.Dominio.Entidades;
using TicketSystem.Infraestructura.Datos;

namespace TicketSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly TicketSystemDbContext _context;
    private readonly IWebHostEnvironment _env;

    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp",
        "application/pdf",
        "text/plain",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };

    private const long MaxBytes = 10 * 1024 * 1024;

    public FilesController(TicketSystemDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost("comment/{commentId}")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadFile(Guid commentId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file received." });

        if (file.Length > MaxBytes)
            return BadRequest(new { message = "The file exceeds the 10 MB limit." });

        if (!AllowedTypes.Contains(file.ContentType))
            return BadRequest(new { message = "File type not allowed." });

        var comment = await _context.TicketComments.FindAsync(commentId);
        if (comment == null)
            return NotFound(new { message = "Comment not found." });

        var uploadsPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsPath);

        var ext = Path.GetExtension(file.FileName);
        var storedName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsPath, storedName);

        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        var attachment = new Attachment
        {
            Id = Guid.NewGuid(),
            CommentId = commentId,
            OriginalName = file.FileName,
            StoredName = storedName,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            UploadedAt = DateTime.UtcNow
        };

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return Ok(new
        {
            success = true,
            data = new
            {
                attachment.Id,
                attachment.OriginalName,
                attachment.ContentType,
                attachment.SizeBytes,
                Url = $"{baseUrl}/uploads/{storedName}"
            }
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> DownloadFile(Guid id)
    {
        var attachment = await _context.Attachments.FindAsync(id);
        if (attachment == null) return NotFound();

        var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", attachment.StoredName);
        if (!System.IO.File.Exists(filePath)) return NotFound();

        var stream = System.IO.File.OpenRead(filePath);
        return File(stream, attachment.ContentType, attachment.OriginalName);
    }
}
