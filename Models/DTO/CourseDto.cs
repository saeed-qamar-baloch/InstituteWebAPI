namespace InstituteWebAPI.Models.DTO
{
    public class CourseDto
    {
        public Guid CourseID { get; set; }

        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public bool CourseStatus { get; set; }
    }
}
