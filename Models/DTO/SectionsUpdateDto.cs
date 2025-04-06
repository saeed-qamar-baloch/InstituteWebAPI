namespace InstituteWebAPI.Models.DTO
{
    public class SectionsUpdateDto
    {
        public string SectionName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid CourseID { get; set; }
        public Guid? TermID { get; set; }
        public Guid? SessionID { get; set; }
    }
}
