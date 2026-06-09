using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public enum TeacherAttendanceStatus
    {
        Present = 0,
        Absent = 1,
        Leave = 2,
        Late = 3
    }

    /// <summary>
    /// Records a teacher's own daily attendance, typically via barcode card scan at reception.
    /// Unique per (TeacherID, AttendanceDate).
    /// </summary>
    public class TeacherDailyAttendance
    {
        [Key]
        public Guid TeacherDailyAttendanceID { get; set; }

        public Guid TeacherID { get; set; }
        [ForeignKey("TeacherID")]
        public Teachers Teacher { get; set; }

        /// <summary>Date of attendance (time component should be 00:00:00).</summary>
        [Column(TypeName = "date")]
        public DateTime AttendanceDate { get; set; }

        public TeacherAttendanceStatus Status { get; set; } = TeacherAttendanceStatus.Present;

        /// <summary>
        /// The raw barcode value scanned from the teacher's ID card.
        /// Null if attendance was entered manually by admin.
        /// </summary>
        public string? ScannedBarcode { get; set; }

        /// <summary>Exact timestamp when the barcode was scanned.</summary>
        public DateTime? ScannedAt { get; set; }

        /// <summary>IdentityUser ID if manually recorded by admin.</summary>
        public string? MarkedByUserID { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
