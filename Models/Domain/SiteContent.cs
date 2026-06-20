using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// Generic CMS store for the public website (rozhn.org).
    /// Each row holds one editable content block as JSON, addressed by Key
    /// (e.g. "institute", "home", "navbar", "gallery", "faq", "curriculum").
    /// </summary>
    [Table("SiteContents", Schema = "web")]
    public class SiteContent
    {
        [Key, MaxLength(100)]
        public string Key { get; set; }

        [Required]
        public string Json { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
