using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class Courses
    {
        [Key]
        public Guid CourseID { get; set; }
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public bool CourseStatus { get; set; }

        public List<Classes> Classes { get; set; }
        public List<Admissions> Admissions { get; set; }
        public List<Slots> Slots { get; set; }
    }
}
