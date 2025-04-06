using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class StudentMarks
    {

        [Key]
        public Guid StudentMarkID { get; set; }
        public float ObtainedMarks{ get; set; }
        public Guid TestID { get; set; }
        [ForeignKey("TestID")]
        public Tests Test { get; set; }
        public Guid StudentID { get; set; }
        [ForeignKey("StudentID")]
        public Students Student { get; set; }
        public Guid TermID { get; set; }
        [ForeignKey("TermID")]
        public Term Term { get; set; }
    }
}
