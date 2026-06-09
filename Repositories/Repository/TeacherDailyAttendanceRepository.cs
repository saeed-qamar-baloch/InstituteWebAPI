using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class TeacherDailyAttendanceRepository : ITeacherDailyAttendanceRepository
    {
        private readonly RozhnInstituteDbContext _db;

        public TeacherDailyAttendanceRepository(RozhnInstituteDbContext db)
        {
            _db = db;
        }

        private IQueryable<TeacherDailyAttendance> BaseQuery() =>
            _db.TeacherDailyAttendances
               .Include(a => a.Teacher)
               .AsNoTracking();

        public async Task<List<TeacherDailyAttendance>> GetAllAsync(
            Guid? teacherId   = null,
            DateTime? fromDate = null,
            DateTime? toDate   = null)
        {
            var q = BaseQuery();

            if (teacherId.HasValue)
                q = q.Where(a => a.TeacherID == teacherId.Value);

            if (fromDate.HasValue)
                q = q.Where(a => a.AttendanceDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                q = q.Where(a => a.AttendanceDate <= toDate.Value.Date);

            return await q
                .OrderByDescending(a => a.AttendanceDate)
                .ThenBy(a => a.Teacher.TeacherName)
                .ToListAsync();
        }

        public async Task<TeacherDailyAttendance?> GetByIdAsync(Guid id) =>
            await BaseQuery().FirstOrDefaultAsync(a => a.TeacherDailyAttendanceID == id);

        public async Task<TeacherDailyAttendance?> GetByTeacherAndDateAsync(Guid teacherId, DateTime date) =>
            await BaseQuery()
                .FirstOrDefaultAsync(a => a.TeacherID == teacherId
                                       && a.AttendanceDate == date.Date);

        public async Task<TeacherDailyAttendance> MarkAsync(
            TeacherDailyAttendance attendance,
            string markedByUserId)
        {
            var date = attendance.AttendanceDate.Date;

            var existing = await _db.TeacherDailyAttendances
                .FirstOrDefaultAsync(a => a.TeacherID == attendance.TeacherID
                                       && a.AttendanceDate == date);

            if (existing == null)
            {
                attendance.TeacherDailyAttendanceID = Guid.NewGuid();
                attendance.AttendanceDate           = date;
                attendance.MarkedByUserID           = markedByUserId;
                attendance.ScannedBarcode           = null;
                attendance.ScannedAt                = null;
                attendance.CreatedOn                = DateTime.UtcNow;
                attendance.UpdatedOn                = null;

                await _db.TeacherDailyAttendances.AddAsync(attendance);
            }
            else
            {
                existing.Status          = attendance.Status;
                existing.Remarks         = attendance.Remarks;
                existing.MarkedByUserID  = markedByUserId;
                existing.UpdatedOn       = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            var savedId = existing?.TeacherDailyAttendanceID ?? attendance.TeacherDailyAttendanceID;
            return (await GetByIdAsync(savedId))!;
        }

        public async Task<TeacherDailyAttendance?> CheckInByBarcodeAsync(
            string barcode,
            TeacherAttendanceStatus status)
        {
            // Barcode on teacher ID card = RegistrationNo
            var teacher = await _db.Teachers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.RegistrationNo == barcode);

            if (teacher == null) return null;

            var today = DateTime.UtcNow.Date;

            var existing = await _db.TeacherDailyAttendances
                .FirstOrDefaultAsync(a => a.TeacherID == teacher.TeacherID
                                       && a.AttendanceDate == today);

            if (existing == null)
            {
                var record = new TeacherDailyAttendance
                {
                    TeacherDailyAttendanceID = Guid.NewGuid(),
                    TeacherID                = teacher.TeacherID,
                    AttendanceDate           = today,
                    Status                   = status,
                    ScannedBarcode           = barcode,
                    ScannedAt                = DateTime.UtcNow,
                    CreatedOn                = DateTime.UtcNow,
                };

                await _db.TeacherDailyAttendances.AddAsync(record);
                await _db.SaveChangesAsync();
                return (await GetByIdAsync(record.TeacherDailyAttendanceID))!;
            }
            else
            {
                // Already scanned today — update status and re-stamp scan time
                existing.Status        = status;
                existing.ScannedBarcode = barcode;
                existing.ScannedAt     = DateTime.UtcNow;
                existing.UpdatedOn     = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return (await GetByIdAsync(existing.TeacherDailyAttendanceID))!;
            }
        }

        public async Task<TeacherDailyAttendance?> DeleteAsync(Guid id)
        {
            var existing = await _db.TeacherDailyAttendances.FindAsync(id);
            if (existing == null) return null;

            _db.TeacherDailyAttendances.Remove(existing);
            await _db.SaveChangesAsync();
            return existing;
        }
    }
}
