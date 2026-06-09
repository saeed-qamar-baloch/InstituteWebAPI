using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ITeacherDailyAttendanceRepository
    {
        /// <summary>Get attendance records. Filter by teacherId and/or date range.</summary>
        Task<List<TeacherDailyAttendance>> GetAllAsync(
            Guid? teacherId   = null,
            DateTime? fromDate = null,
            DateTime? toDate   = null);

        Task<TeacherDailyAttendance?> GetByIdAsync(Guid id);

        Task<TeacherDailyAttendance?> GetByTeacherAndDateAsync(Guid teacherId, DateTime date);

        /// <summary>Admin manually marks attendance (upsert — create or update).</summary>
        Task<TeacherDailyAttendance> MarkAsync(TeacherDailyAttendance attendance, string markedByUserId);

        /// <summary>
        /// Barcode check-in: resolves teacher by RegistrationNo, then upserts today's attendance.
        /// Returns null if the barcode doesn't match any teacher.
        /// </summary>
        Task<TeacherDailyAttendance?> CheckInByBarcodeAsync(string barcode, TeacherAttendanceStatus status);

        Task<TeacherDailyAttendance?> DeleteAsync(Guid id);
    }
}
