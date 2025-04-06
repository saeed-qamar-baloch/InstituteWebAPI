namespace InstituteWebAPI.Models.DTO
{
    public class AddTeacherCourseDto
    {
        public Guid TeacherID { get; set; }
        public Guid CourseID { get; set; }
        public bool CourseIsTaken { get; set; }
    }
}
