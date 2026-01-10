using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.TeacherCourse
{
    public class UpdateTeacherCourseDto
    {
        [Required]
        public Guid TeacherID { get; set; }
        [Required]
        public Guid CourseID { get; set; }
        public bool CourseIsTaken { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
