using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// Tracks changes to a student's monthly fee over time.
    /// Each time the monthly fee is revised a new row is added.
    /// Only one row per AdmissionID should have IsActive = true at any time.
    /// </summary>
    public class StudentFeeHistory
    {
        [Key]
        public Guid FeeHistoryID { get; set; }

        public Guid AdmissionID { get; set; }
        [ForeignKey("AdmissionID")]
        public Admissions Admission { get; set; }

        public Guid CourseID { get; set; }
        [ForeignKey("CourseID")]
        public Courses Course { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FeeAmount { get; set; }

        /// <summary>The date from which this fee amount is effective (first day of relevant month).</summary>
        [Column(TypeName = "date")]
        public DateTime EffectiveFrom { get; set; }

        /// <summary>The date until which this fee was effective. Null = currently active.</summary>
        [Column(TypeName = "date")]
        public DateTime? EffectiveTo { get; set; }

        /// <summary>True for the currently active fee record.</summary>
        public bool IsActive { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
