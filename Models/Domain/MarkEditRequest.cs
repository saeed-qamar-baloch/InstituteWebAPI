using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public enum MarkEditRequestStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }

    /// <summary>
    /// A teacher submits this when they need to change a student's mark after
    /// the admin has approved (locked) the result for that class/term.
    /// Admin reviews and either approves or rejects it.
    /// </summary>
    public class MarkEditRequest
    {
        [Key]
        public Guid RequestID { get; set; }

        public Guid TeacherID { get; set; }
        [ForeignKey("TeacherID")]
        public Teachers Teacher { get; set; }

        public Guid StudentMarkID { get; set; }
        [ForeignKey("StudentMarkID")]
        public StudentMarks StudentMark { get; set; }

        public float CurrentMarks { get; set; }
        public float RequestedMarks { get; set; }

        [Required]
        public string Reason { get; set; }

        public MarkEditRequestStatus Status { get; set; } = MarkEditRequestStatus.Pending;

        /// <summary>IdentityUser ID of the admin who reviewed this request.</summary>
        public string? ReviewedByUserID { get; set; }

        public DateTime? ReviewedAt { get; set; }
        public string? ReviewRemarks { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
