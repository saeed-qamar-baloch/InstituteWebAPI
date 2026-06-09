using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    // Stores per-student monthly aggregate (sum of all tests in a month)
    // Scoped by Term + CurrentClass + TermMonth.
    public class StudentMonthlyResult
    {
        [Key]
        public Guid StudentMonthlyResultID { get; set; }

        public Guid TermID { get; set; }
        [ForeignKey(nameof(TermID))]
        public Term Term { get; set; }

        public Guid CurrentClassID { get; set; }
        [ForeignKey(nameof(CurrentClassID))]
        public CurrentClass CurrentClass { get; set; }

        public Guid TermMonthID { get; set; }
        [ForeignKey(nameof(TermMonthID))]
        public TermMonths TermMonth { get; set; }

        public Guid StudentID { get; set; }
        [ForeignKey(nameof(StudentID))]
        public Students Student { get; set; }

        public float TotalMarks { get; set; }
        public float ObtainedMarks { get; set; }
        public float Percentage { get; set; }

        // Optional status note for the month, e.g. "Not Conducted" when the
        // student has no mark / NI / NC for that month. Null = a normal scored month.
        public string? Status { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }
    }
}
