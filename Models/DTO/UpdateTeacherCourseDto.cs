namespace InstituteWebAPI.Models.DTO
{
    public class UpdateTeacherCourseDto
    {
        public Guid TeacherID { get; set; }
        public Guid CourseID { get; set; }
        public bool CourseIsTaken { get; set; }
    }
}
