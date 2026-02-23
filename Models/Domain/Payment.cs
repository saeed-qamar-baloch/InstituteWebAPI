using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class Payment
    {
        [Key]
        public Guid PaymentId { get; set; }

        public Guid StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public Students Student { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime PaymentDate { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public string? Remarks { get; set; }

        public List<PaymentDetail> PaymentDetails { get; set; } = new();
    }
}
