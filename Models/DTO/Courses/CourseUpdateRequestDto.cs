namespace InstituteWebAPI.Models.DTO.Courses
{
    public class CourseUpdateRequestDto
    {
        public Guid CourseID { get; set; }

        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public bool CourseStatus { get; set; }
    }
}
