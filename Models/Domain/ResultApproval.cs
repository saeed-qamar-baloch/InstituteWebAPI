using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// Tracks whether an admin has approved (locked) the result for a given
    /// class in a given term. Once IsApproved = true, student marks are read-only
    /// and teachers must submit a MarkEditRequest to change them.
    /// Unique per (TermID, CurrentClassID).
    /// </summary>
    public class ResultApproval
    {
        [Key]
        public Guid ApprovalID { get; set; }

        public Guid TermID { get; set; }
        [ForeignKey("TermID")]
        public Term Term { get; set; }

        public Guid CurrentClassID { get; set; }
        [ForeignKey("CurrentClassID")]
        public CurrentClass CurrentClass { get; set; }

        public bool IsApproved { get; set; }

        /// <summary>IdentityUser ID of the admin who approved/locked the result.</summary>
        public string? ApprovedByUserID { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
