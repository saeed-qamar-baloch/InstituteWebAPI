using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.TeacherCourse
{
    public class AddTeacherCourseDto
    {
        [Required]
        public Guid TeacherID { get; set; }
        [Required]
        public Guid CourseID { get; set; }
        public bool CourseIsTaken { get; set; }
    }
}
