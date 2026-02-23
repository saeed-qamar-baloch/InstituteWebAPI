using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class PaymentDetail
    {
        [Key]
        public Guid PaymentDetailId { get; set; }

        public Guid PaymentId { get; set; }
        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; }

        public Guid FeeDueId { get; set; }
        [ForeignKey(nameof(FeeDueId))]
        public FeeDue FeeDue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }
    }
}
