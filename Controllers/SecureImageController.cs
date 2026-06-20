using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    /// <summary>
    /// Serves PRIVATE images (student & teacher photos) ONLY to authenticated
    /// portal staff. These are PII and must never be public — unlike the website
    /// content images under /images/Website and /images/Institute.
    /// </summary>
    [Route("api/secure-image")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher")]
    public class SecureImageController : ControllerBase
    {
        private static readonly string[] AllowedFolders = { "Students", "Teachers" };

        // GET api/secure-image/Students/RZKG-Jan26-001.jpg   (also Teachers)
        [HttpGet("{folder}/{name}")]
        public IActionResult Get([FromRoute] string folder, [FromRoute] string name)
        {
            if (!AllowedFolders.Contains(folder))
                return NotFound();

            // Reject anything that could escape the folder.
            if (string.IsNullOrWhiteSpace(name) || name.Contains("..") ||
                name.Contains('/') || name.Contains('\\'))
                return BadRequest();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Images", folder, name);
            if (!System.IO.File.Exists(path))
                return NotFound();

            var contentType = Path.GetExtension(name).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => "image/jpeg",
            };
            return PhysicalFile(path, contentType);
        }
    }
}
