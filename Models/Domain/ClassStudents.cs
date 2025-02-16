using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class ClassStudents
    {
        [Key]
        public Guid ClassStudentID { get; set; }
        public Guid CurrentClassID { get; set; }
        [ForeignKey("CurrentClassID")]
        public CurrentClass CurrentClass { get; set; }
        public Guid StudentID { get; set; }
        [ForeignKey("StudentID")]
        public Students Student { get; set; }
        public string Status { get; set; }
    }
}
