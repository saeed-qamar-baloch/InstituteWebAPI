using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// A single monthly salary payment for a teacher. The amount is not fixed —
    /// it is entered per payment. One record per teacher per salary month.
    /// </summary>
    public class TeacherSalary
    {
        [Key]
        public Guid TeacherSalaryID { get; set; }

        public Guid TeacherID { get; set; }
        [ForeignKey(nameof(TeacherID))]
        public Teachers Teacher { get; set; }

        /// <summary>The month this salary is for (1–12).</summary>
        public int SalaryMonth { get; set; }

        /// <summary>The year this salary is for.</summary>
        public int SalaryYear { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>The date the salary was actually paid.</summary>
        public DateTime PaidOn { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
