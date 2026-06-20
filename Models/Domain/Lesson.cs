using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// A "Learn English" lesson, authored as a list of content blocks (BlocksJson).
    /// Lives in the separate "web" schema alongside the other public-website content
    /// (WebsitePost, SiteContent). The Slug mirrors the curriculum tree,
    /// e.g. "grammar/present-simple" or "basics/the-alphabet".
    /// </summary>
    [Table("Lessons", Schema = "web")]
    public class Lesson
    {
        [Key]
        public Guid LessonID { get; set; }

        /// <summary>Curriculum path, e.g. "grammar/present-simple". Unique.</summary>
        [Required, MaxLength(200)]
        public string Slug { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>Curriculum section label, e.g. "Tenses".</summary>
        [MaxLength(120)]
        public string Section { get; set; } = string.Empty;

        /// <summary>Grammar, Vocabulary, Writing, Speaking, Reading, Listening, Pronunciation.</summary>
        [MaxLength(60)]
        public string Category { get; set; } = string.Empty;

        public bool IsPopular { get; set; } = false;
        public bool IsPractice { get; set; } = false;

        /// <summary>Learning path / level, e.g. "Beginner", "Intermediate", "Advanced".</summary>
        [MaxLength(30)]
        public string Level { get; set; } = string.Empty;

        public int SectionOrder { get; set; } = 0;

        public int Order { get; set; } = 0;

        /// <summary>JSON array of content blocks (rendered by the website LessonBlocks component).</summary>
        [Required]
        public string BlocksJson { get; set; } = "[]";

        public bool IsPublished { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
