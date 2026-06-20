using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Lessons
{
    public class AddLessonDto
    {
        [Required, MaxLength(200)]
        public string Slug { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(120)]
        public string? Section { get; set; }

        [MaxLength(60)]
        public string? Category { get; set; }

        public bool IsPopular { get; set; } = false;
        public bool IsPractice { get; set; } = false;

        [MaxLength(30)]
        public string? Level { get; set; }

        public int SectionOrder { get; set; } = 0;

        public int Order { get; set; } = 0;

        [Required]
        public string BlocksJson { get; set; } = "[]";

        public bool IsPublished { get; set; } = true;
    }
}
