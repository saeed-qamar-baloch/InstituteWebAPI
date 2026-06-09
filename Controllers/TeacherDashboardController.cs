using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class TeacherDashboardController : ControllerBase
    {
        private readonly RozhnInstituteDbContext db;
        private readonly ITeacherIdentityLinkRepository teacherIdentity;

        public TeacherDashboardController(RozhnInstituteDbContext db, ITeacherIdentityLinkRepository teacherIdentity)
        {
            this.db = db;
            this.teacherIdentity = teacherIdentity;
        }

        private async Task<Guid?> GetTeacherIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return null;
            return await teacherIdentity.GetTeacherIdForUserIdAsync(userId);
        }

        [HttpGet]
        public async Task<IActionResult> GetSummary()
        {
            var teacherId = await GetTeacherIdAsync();
            if (teacherId == null) return Forbid();

            var today = DateTime.Today;

            // Active term (fall back to the most recently started term).
            var activeTerm = await db.Term.AsNoTracking()
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.TermStart)
                .FirstOrDefaultAsync()
                ?? await db.Term.AsNoTracking()
                .OrderByDescending(t => t.TermStart)
                .FirstOrDefaultAsync();

            // My current classes — active term only
            var myClasses = activeTerm == null
                ? new List<CurrentClass>()
                : await db.CurrentClasses
                    .AsNoTracking()
                    .Include(cc => cc.Class)
                    .Include(cc => cc.Slot)
                    .Include(cc => cc.Section)
                    .Include(cc => cc.Term)
                    .Where(cc => cc.TeacherID == teacherId.Value && cc.TermID == activeTerm.TermID)
                    .ToListAsync();

            var classIds = myClasses.Select(cc => cc.CurrentClassID).ToList();

            // Student count per class
            var studentCounts = await db.ClassStudents
                .AsNoTracking()
                .Where(cs => classIds.Contains(cs.CurrentClassID))
                .GroupBy(cs => cs.CurrentClassID)
                .Select(g => new { ClassID = g.Key, Count = g.Count() })
                .ToListAsync();

            var countMap = studentCounts.ToDictionary(x => x.ClassID, x => x.Count);
            var totalStudents = studentCounts.Sum(x => x.Count);

            // My attendance today
            var myAttendance = await db.TeacherDailyAttendances
                .AsNoTracking()
                .Where(a => a.TeacherID == teacherId.Value && a.AttendanceDate == today)
                .OrderByDescending(a => a.CreatedOn)
                .FirstOrDefaultAsync();

            // Pending leave requests I submitted
            var pendingLeaveRequests = await db.StudentLeaveRequests
                .AsNoTracking()
                .CountAsync(r => r.RequestedByTeacherID == teacherId.Value
                              && r.Status == LeaveRequestStatus.Pending);

            // Classes data
            var classesData = myClasses.Select(cc => new
            {
                cc.CurrentClassID,
                ClassName   = cc.Class?.ClassName ?? "—",
                SlotName    = cc.Slot?.SlotName ?? "—",
                StartTime   = cc.Slot?.StartTime,
                EndTime     = cc.Slot?.EndTime,
                Section     = cc.Section?.Name,
                TermName    = cc.Term?.TermName,
                StudentCount = countMap.TryGetValue(cc.CurrentClassID, out var cnt) ? cnt : 0,
            }).OrderBy(c => c.StartTime).ToList();

            return Ok(new
            {
                totalClasses     = myClasses.Count,
                totalStudents,
                pendingLeaveRequests,
                myAttendanceToday = myAttendance == null ? null : new
                {
                    status    = myAttendance.Status.ToString(),
                    scannedAt = myAttendance.ScannedAt,
                },
                classes = classesData,
            });
        }
    }
}
