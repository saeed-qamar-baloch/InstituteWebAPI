using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// A record that an admit card was generated for a student in a class,
    /// capturing the unpaid-month count at generation time.
    /// </summary>
    public class AdmitCard
    {
        [Key]
        public Guid AdmitCardID { get; set; }

        public Guid StudentID { get; set; }
        [ForeignKey(nameof(StudentID))]
        public Students Student { get; set; }

        public Guid CurrentClassID { get; set; }
        [ForeignKey(nameof(CurrentClassID))]
        public CurrentClass CurrentClass { get; set; }

        public Guid? TermID { get; set; }

        /// <summary>Unpaid monthly-fee count for the student when this card was generated.</summary>
        public int UnpaidMonths { get; set; }

        public DateTime GeneratedOn { get; set; } = DateTime.UtcNow;
    }
}
