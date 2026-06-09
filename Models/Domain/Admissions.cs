using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public enum AdmissionStatus
    {
        Active = 1,
        Inactive = 2,
        Left = 3
    }

    public class Admissions
    {
        [Key]
        public Guid AdmissionID { get; set; }

        public Guid StudentID { get; set; }
        [ForeignKey("StudentID")]
        public Students Student { get; set; }

        public DateTime RegistrationDate { get; set; }

        public Guid CourseID { get; set; }
        [ForeignKey("CourseID")]
        public Courses Course { get; set; }

        /// <summary>
        /// The class the student was placed into at admission time.
        /// Fixed historical reference — never changes after promotions.
        /// </summary>
        public Guid? AdmittedClassID { get; set; }
        [ForeignKey("AdmittedClassID")]
        public Classes? AdmittedClass { get; set; }

        public DateTime? LeavingDate { get; set; }

        // Monthly fee for this admission (set at admission time)
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyFee { get; set; }

        // One-time admission / registration fee
        [Column(TypeName = "decimal(18,2)")]
        public decimal AdmissionFee { get; set; }

        // Due date as day-of-month (1-31)
        public int? DueDate { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        /// <summary>Use AdmissionStatus enum: Active, Inactive, Left.</summary>
        public string Status { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// When true, the student is not charged any fee — no monthly or admission
        /// dues are generated for this admission.
        /// </summary>
        public bool IsFree { get; set; }

        // Navigation properties
        public List<FeeDue> FeeDues { get; set; } = new();
        public List<Scholarship> Scholarships { get; set; } = new();
        public List<StudentFeeHistory> FeeHistories { get; set; } = new();
    }
}
