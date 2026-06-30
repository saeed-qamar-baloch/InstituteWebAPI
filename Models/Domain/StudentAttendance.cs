using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public enum AttendanceStatus
    {
        Present = 0,
        Absent = 1,
        Leave = 2,
        Late = 3,

        /// <summary>
        /// Not Registered — the attendance date falls before the student's
        /// current admission RegistrationDate, so attendance does not apply.
        /// Mirrors the FeeDueStatus.NR concept used in the fee module.
        /// </summary>
        NR = 4
    }

    public class StudentAttendance
    {
        [Key]
        public Guid StudentAttendanceID { get; set; }

        // Attendance date (date-only semantics; time should be 00:00:00)
        public DateTime AttendanceDate { get; set; }

        public Guid CurrentClassID { get; set; }
        [ForeignKey(nameof(CurrentClassID))]
        public CurrentClass CurrentClass { get; set; }

        public Guid StudentID { get; set; }
        [ForeignKey(nameof(StudentID))]
        public Students Student { get; set; }

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        public Guid? MarkedByTeacherID { get; set; }
        [ForeignKey(nameof(MarkedByTeacherID))]
        public Teachers? MarkedByTeacher { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }
    }
}
