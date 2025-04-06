namespace InstituteWebAPI.Models.DTO
{
    public class SectionsDto
    {
        public Guid SectionID { get; set; }
        public string SectionName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public CourseDto Course { get; set; }
        public TermDto? Term { get; set; }
        public SessionsDto? Session { get; set; }
    }
}
