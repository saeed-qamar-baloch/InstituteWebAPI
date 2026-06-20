using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// Content for the public website (rozhn.org).
    /// PostType: "Post" = news/announcement, "Achievement" = achievement card,
    /// "Page" = standalone website page (rendered at rozhn.org/p/{slug}).
    /// Lives in the separate "web" schema to keep public-website content
    /// isolated from institute management tables (dbo).
    /// </summary>
    [Table("WebsitePosts", Schema = "web")]
    public class WebsitePost
    {
        [Key]
        public Guid WebsitePostID { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        /// <summary>"Post", "Achievement" or "Page"</summary>
        [Required, MaxLength(20)]
        public string PostType { get; set; } = "Post";

        /// <summary>URL slug for pages (e.g. "about-us"). Empty for posts/achievements.</summary>
        [MaxLength(200)]
        public string Slug { get; set; } = string.Empty;

        /// <summary>Absolute URL of the uploaded image (empty when none).</summary>
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsPublished { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public IFormFile? file { get; set; }
    }
}
