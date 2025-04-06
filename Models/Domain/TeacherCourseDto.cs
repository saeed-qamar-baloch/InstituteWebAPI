namespace InstituteWebAPI.Models.DTO
{
    public class TeacherCourseDto
    {
        public Guid TeacherCourseID { get; set; }
        public CourseDto Course { get; set; }
        public TeacherDto Teacher { get; set; }
        public bool CourseIsTaken { get; set; }
    }
}
