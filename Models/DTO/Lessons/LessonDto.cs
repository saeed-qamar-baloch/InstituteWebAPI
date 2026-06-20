namespace InstituteWebAPI.Models.DTO.Lessons
{
    public class LessonDto
    {
        public Guid LessonID { get; set; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Section { get; set; }
        public string Category { get; set; }
        public bool IsPopular { get; set; }
        public bool IsPractice { get; set; }
        public string Level { get; set; }
        public int SectionOrder { get; set; }
        public int Order { get; set; }
        public string BlocksJson { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
