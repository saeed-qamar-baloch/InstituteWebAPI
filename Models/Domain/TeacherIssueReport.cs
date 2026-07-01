using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public enum IssueType
    {
        InfoCorrection              = 1,
        StudentDoesntBelongToClass  = 2,
        StudentNotListedInClass     = 3,
        OtherIssue                  = 4,
    }

    public enum IssueStatus
    {
        Open       = 1,
        InProgress = 2,
        Resolved   = 3,
        Dismissed  = 4,
    }

    /// <summary>
    /// A report submitted by a teacher to flag a class or student-related issue
    /// (e.g. wrong info, student enrolled in wrong class, student missing from roster).
    /// Admins review, update the status, and optionally add notes.
    /// </summary>
    public class TeacherIssueReport
    {
        [Key]
        public Guid IssueId { get; set; }

        public Guid TeacherId { get; set; }
        [ForeignKey(nameof(TeacherId))]
        public Teachers Teacher { get; set; } = null!;

        /// <summary>The class (CurrentClass) the issue is about.</summary>
        public Guid CurrentClassId { get; set; }
        [ForeignKey(nameof(CurrentClassId))]
        public CurrentClass CurrentClass { get; set; } = null!;

        /// <summary>Optional — the specific student the issue concerns.</summary>
        public Guid? StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public Students? Student { get; set; }

        public IssueType   IssueType   { get; set; }
        public IssueStatus Status      { get; set; } = IssueStatus.Open;

        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AdminNotes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
