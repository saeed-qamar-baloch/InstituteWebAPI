using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// A teacher's request for teaching material (books, wordlists, supplies, etc.).
    /// Status: Pending -> Approved -> Fulfilled, or Pending -> Rejected.
    /// Admins default to seeing only Pending requests; everything else is "inactive" history.
    /// </summary>
    public class MaterialRequest
    {
        [Key]
        public Guid MaterialRequestID { get; set; }

        public Guid TeacherID { get; set; }
        [ForeignKey(nameof(TeacherID))]
        public Teachers Teacher { get; set; }

        [Required]
        [MaxLength(200)]
        public string MaterialName { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;

        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>Pending | Approved | Fulfilled | Rejected</summary>
        public string Status { get; set; } = "Pending";

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedOn { get; set; }
        public DateTime? FulfilledOn { get; set; }

        /// <summary>Optional admin note — e.g. rejection reason or fulfilment detail.</summary>
        [MaxLength(500)]
        public string? AdminNote { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
