using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    // Stores terminal result inclusion choices per student (month1/month2 optional, month3 required)
    public class TerminalResult
    {
        [Key]
        public Guid TerminalResultID { get; set; }

        public Guid TermID { get; set; }
        [ForeignKey(nameof(TermID))]
        public Term Term { get; set; }

        public Guid CurrentClassID { get; set; }
        [ForeignKey(nameof(CurrentClassID))]
        public CurrentClass CurrentClass { get; set; }

        public Guid StudentID { get; set; }
        [ForeignKey(nameof(StudentID))]
        public Students Student { get; set; }

        // The test IDs used as terminal's month tests.
        // Month3 is mandatory.
        public Guid Month3TestID { get; set; }

        public Guid? Month1TestID { get; set; }
        public bool IncludeMonth1 { get; set; }

        public Guid? Month2TestID { get; set; }
        public bool IncludeMonth2 { get; set; }

        // Persisted calculated values
        public float TotalMarksConsidered { get; set; }
        public float TotalObtained { get; set; }
        public float Percentage { get; set; }
        public string? Result { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }
    }
}
