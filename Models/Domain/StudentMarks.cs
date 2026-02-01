using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class StudentMarks
    {

        [Key]
        public Guid StudentMarkID { get; set; }

        // Per-test obtained marks
        public float ObtainedMarks { get; set; }

        // Denormalized per-test totals (copied from `Tests.TotalMarks` at save-time)
        public float TotalMarks { get; set; }

        // Denormalized per-test percentage (ObtainedMarks/TotalMarks*100)
        public float Percentage { get; set; }

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
