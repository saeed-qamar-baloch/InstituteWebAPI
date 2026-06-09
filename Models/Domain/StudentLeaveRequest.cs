using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public enum LeaveRequestStatus
    {
        Pending  = 0,
        Approved = 1,
        Rejected = 2,
    }

    public class StudentLeaveRequest
    {
        [Key]
        public Guid StudentLeaveRequestID { get; set; }

        // The student being requested to mark as left
        public Guid StudentID { get; set; }
        [ForeignKey("StudentID")]
        public Students Student { get; set; }

        // Which class the student is in
        public Guid CurrentClassID { get; set; }
        [ForeignKey("CurrentClassID")]
        public CurrentClass CurrentClass { get; set; }

        // The active admission for this student
        public Guid AdmissionID { get; set; }
        [ForeignKey("AdmissionID")]
        public Admissions Admission { get; set; }

        // The teacher who submitted the request
        public Guid RequestedByTeacherID { get; set; }
        [ForeignKey("RequestedByTeacherID")]
        public Teachers RequestedByTeacher { get; set; }

        [Required]
        public DateTime LeavingDate { get; set; }

        [Required]
        public string Reason { get; set; }

        public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

        public string? ReviewedByUserID { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewRemarks { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
