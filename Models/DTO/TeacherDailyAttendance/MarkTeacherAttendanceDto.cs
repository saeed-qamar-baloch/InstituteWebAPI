using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.TeacherDailyAttendance
{
    /// <summary>Admin manually marks or updates attendance for one or more teachers on a date.</summary>
    public class MarkTeacherAttendanceDto
    {
        [Required]
        public Guid TeacherID { get; set; }

        [Required]
        public DateTime AttendanceDate { get; set; }

        /// <summary>0=Present, 1=Absent, 2=Leave, 3=Late</summary>
        [Required]
        [Range(0, 3)]
        public int Status { get; set; }

        public string? Remarks { get; set; }
    }
}
