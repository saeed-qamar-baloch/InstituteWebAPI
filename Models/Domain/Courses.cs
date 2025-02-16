using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    public class Courses
    {
        [Key]
        public Guid CourseID { get; set; }
        
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public bool CourseStatus { get; set; } 

        public List<Classes> Classes { get ; set; }
        public List<Sections> Sections { get; set; } 
      
        public List<StudentCourses> StudentCourses { get; set; }
    }
}
