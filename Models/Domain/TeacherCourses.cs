using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class TeacherCourses
    {
        [Key]
        public Guid TeacherCourseID { get; set; }

        public Guid TeacherID { get; set; }
        [ForeignKey("TeacherID")]
        public Teachers Teacher { get; set; }

        public Guid CourseID { get; set; }
        [ForeignKey("CourseID")]
        public Courses Course { get; set; }

        public bool CourseIsTaken { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
