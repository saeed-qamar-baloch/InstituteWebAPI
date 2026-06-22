using InstituteWebAPI.Data;
using InstituteWebAPI.Services.Storage;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher")]
    public class AdminInstituteSettingController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly ImageStorage imageStorage;
        private readonly IHttpContextAccessor http;

        public AdminInstituteSettingController(RozhnInstituteDbContext dbContext, ImageStorage imageStorage, IHttpContextAccessor http)
        {
            this.dbContext = dbContext;
            this.imageStorage = imageStorage;
            this.http = http;
        }

        public class SettingsDto
        {
            public List<int> OffDays { get; set; } = new();
            public string? InstituteName { get; set; }
            public string? LogoUrl { get; set; }
            public string? Tagline { get; set; }
            public string? Address { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public string? Website { get; set; }
            public string? SinceYear { get; set; }
        }
        public class OffDaysDto { public List<int> OffDays { get; set; } = new(); }
        public class InfoDto
        {
            public string? InstituteName { get; set; }
            public string? Tagline { get; set; }
            public string? Address { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public string? Website { get; set; }
            public string? SinceYear { get; set; }
        }

        private static List<int> Parse(string? csv) =>
            string.IsNullOrWhiteSpace(csv) ? new()
                : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                     .Select(x => int.TryParse(x, out var n) ? n : 0)
                     .Where(n => n >= 1 && n <= 7).Distinct().ToList();

        private string? ExistingLogoUrl(string? logoUrl)
        {
            if (string.IsNullOrWhiteSpace(logoUrl)) return null;

            var path = Uri.TryCreate(logoUrl, UriKind.Absolute, out var uri)
                ? uri.AbsolutePath
                : logoUrl.Split('?', '#')[0];
            var fileName = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(fileName)) return null;

            return System.IO.File.Exists(Path.Combine(imageStorage.GetFolder("Institute"), fileName))
                ? logoUrl
                : null;
        }

        private async Task<InstituteSetting> GetOrCreateAsync()
        {
            var s = await dbContext.InstituteSettings.FirstOrDefaultAsync();
            if (s == null)
            {
                s = new InstituteSetting { InstituteSettingID = Guid.NewGuid(), OffDays = "7", InstituteName = "Rozhn Institute" };
                dbContext.InstituteSettings.Add(s);
                await dbContext.SaveChangesAsync();
            }
            return s;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var s = await GetOrCreateAsync();
            return Ok(new SettingsDto
            {
                OffDays = Parse(s.OffDays),
                InstituteName = s.InstituteName ?? "Rozhn Institute",
                LogoUrl = ExistingLogoUrl(s.LogoUrl),
                Tagline = s.Tagline,
                Address = s.Address,
                Phone = s.Phone,
                Email = s.Email,
                Website = s.Website,
                SinceYear = s.SinceYear,
            });
        }

        [HttpPut("offdays")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOffDays([FromBody] OffDaysDto dto)
        {
            var days = (dto.OffDays ?? new()).Where(d => d >= 1 && d <= 7).Distinct().ToList();
            if (days.Count > 2) return BadRequest(new { message = "At most 2 off days are allowed." });
            var s = await GetOrCreateAsync();
            s.OffDays = string.Join(",", days);
            await dbContext.SaveChangesAsync();
            return Ok(new { OffDays = days });
        }

        [HttpPut("info")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateInfo([FromBody] InfoDto dto)
        {
            var s = await GetOrCreateAsync();
            s.InstituteName = string.IsNullOrWhiteSpace(dto.InstituteName) ? "Rozhn Institute" : dto.InstituteName.Trim();
            s.Tagline   = dto.Tagline?.Trim();
            s.Address   = dto.Address?.Trim();
            s.Phone     = dto.Phone?.Trim();
            s.Email     = dto.Email?.Trim();
            s.Website   = dto.Website?.Trim();
            s.SinceYear = dto.SinceYear?.Trim();
            await dbContext.SaveChangesAsync();
            return Ok(new { s.InstituteName });
        }

        [HttpPost("logo")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { message = "No file uploaded." });
            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".png";

            var dir = imageStorage.GetFolder("Institute");
            Directory.CreateDirectory(dir);
            var fileName = $"logo{ext.ToLowerInvariant()}";
            var path = Path.Combine(dir, fileName);
            using (var fs = new FileStream(path, FileMode.Create))
                await file.CopyToAsync(fs);

            var req = http.HttpContext!.Request;
            var url = $"{req.Scheme}://{req.Host}{req.PathBase}/images/Institute/{fileName}?v={DateTime.UtcNow.Ticks}";

            var s = await GetOrCreateAsync();
            s.LogoUrl = url;
            await dbContext.SaveChangesAsync();
            return Ok(new { s.LogoUrl });
        }
    }
}
