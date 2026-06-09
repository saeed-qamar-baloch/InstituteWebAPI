using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public enum ScholarshipStatus
    {
        Active = 1,
        Inactive = 2
    }

    /// <summary>
    /// Fee discount awarded to a student for good academic performance.
    /// DiscountPercent is one of 25, 50, or 100.
    /// Applies for the months between FromMonth and ToMonth.
    /// </summary>
    public class Scholarship
    {
        [Key]
        public Guid ScholarshipID { get; set; }

        public Guid StudentID { get; set; }
        [ForeignKey("StudentID")]
        public Students Student { get; set; }

        public Guid AdmissionID { get; set; }
        [ForeignKey("AdmissionID")]
        public Admissions Admission { get; set; }

        /// <summary>25, 50, 75, or 100 percent discount on monthly fee.</summary>
        public int DiscountPercent { get; set; }

        /// <summary>
        /// True when this concession is a leave period (fee fully waived,
        /// DiscountPercent = 100). Distinguishes leave from merit scholarships.
        /// </summary>
        public bool IsLeave { get; set; }

        /// <summary>First month the discount applies (first day of the month).</summary>
        [Column(TypeName = "date")]
        public DateTime FromMonth { get; set; }

        /// <summary>Last month the discount applies (first day of the month).</summary>
        [Column(TypeName = "date")]
        public DateTime ToMonth { get; set; }

        public string? Reason { get; set; }

        public ScholarshipStatus Status { get; set; } = ScholarshipStatus.Active;

        /// <summary>IdentityUser ID of the admin who created this scholarship.</summary>
        public string? CreatedByUserID { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
