using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// A student's ID card request with its own payment tracking.
    /// CardType: New | Replacement. Status: Requested | Paid | Delivered.
    /// </summary>
    public class CardRequest
    {
        [Key]
        public Guid CardRequestID { get; set; }

        public Guid StudentID { get; set; }
        [ForeignKey(nameof(StudentID))]
        public Students Student { get; set; }

        public string CardType { get; set; } = "New";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string Status { get; set; } = "Requested";

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? PaidOn { get; set; }
        public DateTime? DeliveredOn { get; set; }

        public string? Notes { get; set; }

        // Optional: the teacher who raised the request (null = raised by an admin).
        public Guid? RequestedByTeacherID { get; set; }
        [ForeignKey(nameof(RequestedByTeacherID))]
        public Teachers? RequestedByTeacher { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
