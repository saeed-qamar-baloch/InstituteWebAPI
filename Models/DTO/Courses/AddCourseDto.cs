using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Courses
{
    public class AddCourseDto
    {
        // public Guid CourseID { get; set; }
        [Required]
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public bool CourseStatus { get; set; }
    }
}
