using InstituteWebAPI.Data;
using InstituteWebAPI.Services.Storage;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Controllers
{
    /// <summary>Admin editing of website content blocks (rozhn.org CMS).</summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminSiteContentController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly ImageStorage imageStorage;

        public AdminSiteContentController(RozhnInstituteDbContext dbContext, ImageStorage imageStorage)
        {
            this.dbContext = dbContext;
            this.imageStorage = imageStorage;
        }

        // GET api/AdminSiteContent — all blocks
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rows = await dbContext.SiteContents.ToListAsync();
            return Ok(rows.Select(r => new { key = r.Key, json = r.Json, updatedAt = r.UpdatedAt }));
        }

        // GET api/AdminSiteContent/{key}
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            var row = await dbContext.SiteContents.FindAsync(key);
            if (row == null) return NotFound();
            return Ok(new { key = row.Key, json = row.Json, updatedAt = row.UpdatedAt });
        }

        public class SaveContentDto { public string Json { get; set; } = "{}"; }

        // PUT api/AdminSiteContent/{key} — upsert
        [HttpPut("{key}")]
        public async Task<IActionResult> Save(string key, [FromBody] SaveContentDto dto)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length > 100)
                return BadRequest("Invalid key.");
            try { System.Text.Json.JsonDocument.Parse(dto.Json); }
            catch { return BadRequest("Json field is not valid JSON."); }

            var row = await dbContext.SiteContents.FindAsync(key);
            if (row == null)
            {
                row = new SiteContent { Key = key, Json = dto.Json, UpdatedAt = DateTime.UtcNow };
                await dbContext.SiteContents.AddAsync(row);
            }
            else
            {
                row.Json = dto.Json;
                row.UpdatedAt = DateTime.UtcNow;
            }
            await dbContext.SaveChangesAsync();
            return Ok(new { key = row.Key, updatedAt = row.UpdatedAt });
        }

        // POST api/AdminSiteContent/upload — generic image upload, returns { url }
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file.");
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowed.Contains(ext)) return BadRequest("Only image files are allowed.");

            var name = $"content-{Guid.NewGuid()}{ext}";
            var path = Path.Combine(imageStorage.GetFolder("Website"), name);
            using (var fs = new FileStream(path, FileMode.Create))
                await file.CopyToAsync(fs);

            var url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/images/Website/{name}";
            return Ok(new { url });
        }
    }
}
