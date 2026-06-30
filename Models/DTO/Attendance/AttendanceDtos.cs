using System.ComponentModel.DataAnnotations;
using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Models.DTO.Attendance
{
    public class AttendanceStudentRowDto
    {
        public Guid StudentID { get; set; }
        public string? RegistrationNo { get; set; }
        public string? StudentName { get; set; }
        public string? FatherName { get; set; }

        // Null means the teacher has not marked this student yet for the
        // selected date (distinct from AttendanceStatus.Present).
        public AttendanceStatus? Status { get; set; }
    }

    public class AttendanceSheetDto
    {
        public DateTime Date { get; set; }
        public Guid CurrentClassID { get; set; }
        public bool IsMarked { get; set; }
        public List<AttendanceStudentRowDto> Students { get; set; } = new();
    }

    public class SaveAttendanceDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public Guid CurrentClassID { get; set; }

        [Required]
        public List<SaveAttendanceStudentDto> Students { get; set; } = new();
    }

    public class SaveAttendanceStudentDto
    {
        [Required]
        public Guid StudentID { get; set; }

        // Null means this student was never marked by the teacher for this
        // date — the row is skipped on save rather than persisted as Present.
        public AttendanceStatus? Status { get; set; }
    }

    public class StudentAttendanceCalendarDto
    {
        public int Year { get; set; }
        public List<StudentAttendanceMonthDto> Months { get; set; } = new();
    }

    public class StudentAttendanceMonthDto
    {
        public int Month { get; set; }
        public List<string?> Days { get; set; } = new();
    }
}
