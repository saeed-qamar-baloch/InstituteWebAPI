using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class FeeDue
    {
        [Key]
        public Guid FeeDueId { get; set; }

        public Guid AdmissionId { get; set; }
        [ForeignKey(nameof(AdmissionId))]
        public Admissions Admission { get; set; }

        public FeeDueType FeeType { get; set; }

        [Column(TypeName = "date")]
        public DateTime? FeeMonth { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LateFeeAmount { get; set; }

        [Column(TypeName = "date")]
        public DateTime DueDate { get; set; }

        public bool IsLateFeeWaived { get; set; }

        public FeeDueStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<PaymentDetail> PaymentDetails { get; set; } = new();
    }
}
