namespace InstituteWebAPI.Models.DTO.TeacherDailyAttendance
{
    public class TeacherDailyAttendanceDto
    {
        public Guid TeacherDailyAttendanceID { get; set; }
        public Guid TeacherID { get; set; }
        public string? TeacherName { get; set; }
        public string? RegistrationNo { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Status { get; set; }          // "Present" | "Absent" | "Leave" | "Late"
        public string? ScannedBarcode { get; set; }
        public DateTime? ScannedAt { get; set; }
        public string? MarkedByUserID { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
