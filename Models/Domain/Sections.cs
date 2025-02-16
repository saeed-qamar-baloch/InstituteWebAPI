using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class Sections
    {
        [Key]
        public Guid SectionID { get; set; }
        public string SectionName { get; set; }
        public Guid CourseID { get; set; }
        [ForeignKey("CourseID")]
        public Courses Course { get; set; }
        public bool IsDeleted { get; set; }
        public List<CurrentClass> CurrentClasses { get; set; }
    }
}
