using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class Classes
    {
        [Key]
        public Guid ClassID { get; set; }

        public string ClassName { get; set; }
        public Guid CourseID { get; set; }
        [ForeignKey("CourseID")]
        public Courses Course { get; set; }
        public List<CurrentClass> CurrentClasses { get; set; }
    }
}
