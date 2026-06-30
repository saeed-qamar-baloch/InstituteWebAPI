using InstituteWebAPI.Data;
using InstituteWebAPI.Services.TermContext;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly ITermContext termContext;

        public DashboardController(RozhnInstituteDbContext dbContext, ITermContext termContext)
        {
            this.dbContext   = dbContext;
            this.termContext = termContext;
        }

        // ── GET api/dashboard ─────────────────────────────────────────────────
        // Returns key metrics for the admin home screen in a single round-trip.
        [HttpGet]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.UtcNow.Date;
            var now   = DateTime.UtcNow;

            // ── Students ──────────────────────────────────────────────────────
            var totalStudents   = await dbContext.Students.CountAsync();
            var enrolledStudents = await dbContext.Students.CountAsync(s => s.IsEnrolled);

            // ── Active term ───────────────────────────────────────────────────
            Term? activeTerm = null;
            try { activeTerm = await termContext.GetActiveTermAsync(); }
            catch { /* no active term configured — non-fatal */ }

            // ── Fee stats ─────────────────────────────────────────────────────
            // Today's collection
            var todayPayments = await dbContext.PaymentDetails
                .AsNoTracking()
                .Include(pd => pd.Payment)
                .Where(pd => pd.Payment.PaymentDate.Date == today)
                .SumAsync(pd => (decimal?)pd.PaidAmount) ?? 0m;

            // This month's collection
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthPayments = await dbContext.PaymentDetails
                .AsNoTracking()
                .Include(pd => pd.Payment)
                .Where(pd => pd.Payment.PaymentDate >= monthStart)
                .SumAsync(pd => (decimal?)pd.PaidAmount) ?? 0m;

            // Defaulters: active admissions with overdue Unpaid/Partial dues.
            // A due with nothing actually owed (waived, or a 0-amount fee) is never a
            // real default, even if its stored Status hasn't been self-healed to
            // Waived yet by GetUnpaidDuesAsync.
            var defaulterCount = await dbContext.FeeDues
                .AsNoTracking()
                .Include(d => d.Admission)
                .Where(d =>
                    d.DueDate < today &&
                    d.Admission.IsActive &&
                    (d.Status == FeeDueStatus.Unpaid || d.Status == FeeDueStatus.Partial) &&
                    (d.BaseAmount + (d.IsLateFeeWaived ? 0m : d.LateFeeAmount)) > 0m &&
                    d.Status != FeeDueStatus.NR)
                .Select(d => d.Admission.StudentID)
                .Distinct()
                .CountAsync();

            // Total outstanding balance (across all active admissions)
            var allDues = await dbContext.FeeDues
                .AsNoTracking()
                .Include(d => d.Admission)
                .Include(d => d.PaymentDetails)
                .Where(d => d.Admission.IsActive &&
                            (d.Status == FeeDueStatus.Unpaid || d.Status == FeeDueStatus.Partial) &&
                            (d.BaseAmount + (d.IsLateFeeWaived ? 0m : d.LateFeeAmount)) > 0m &&
                            d.Status != FeeDueStatus.NR)
                .ToListAsync();

            var outstandingBalance = allDues.Sum(d =>
                (d.BaseAmount + (d.IsLateFeeWaived ? 0m : d.LateFeeAmount))
                - d.PaymentDetails.Sum(p => p.PaidAmount));

            // ── Attendance — today ────────────────────────────────────────────
            var todayAttendance = await dbContext.StudentAttendances
                .AsNoTracking()
                .Where(a => a.AttendanceDate == today)
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var presentToday = todayAttendance
                .Where(x => x.Status == AttendanceStatus.Present || x.Status == AttendanceStatus.Late)
                .Sum(x => x.Count);
            var absentToday = todayAttendance
                .FirstOrDefault(x => x.Status == AttendanceStatus.Absent)?.Count ?? 0;

            // Teacher attendance today
            var teacherAttendanceToday = await dbContext.TeacherDailyAttendances
                .AsNoTracking()
                .Where(a => a.AttendanceDate == today)
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            var totalMarkedToday = todayAttendance.Sum(x => x.Count);
            var attendancePctToday = totalMarkedToday > 0
                ? Math.Round((double)presentToday / totalMarkedToday * 100, 1) : 0.0;

            // ── Collection rate (active admissions, all-time) ─────────────────
            var baseBilled = await dbContext.FeeDues.AsNoTracking()
                .Where(d => d.Admission.IsActive)
                .SumAsync(d => (decimal?)d.BaseAmount) ?? 0m;
            var lateBilled = await dbContext.FeeDues.AsNoTracking()
                .Where(d => d.Admission.IsActive && !d.IsLateFeeWaived)
                .SumAsync(d => (decimal?)d.LateFeeAmount) ?? 0m;
            var totalBilled = baseBilled + lateBilled;
            var totalCollected = await dbContext.PaymentDetails.AsNoTracking()
                .Where(pd => pd.FeeDue.Admission.IsActive)
                .SumAsync(pd => (decimal?)pd.PaidAmount) ?? 0m;
            var collectionRate = totalBilled > 0
                ? Math.Round((double)(totalCollected / totalBilled) * 100, 1) : 0.0;

            // ── Expenses + net profit this month ──────────────────────────────
            var expensesThisMonth = await dbContext.Expenses.AsNoTracking()
                .Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate <= today)
                .SumAsync(e => (decimal?)e.Amount) ?? 0m;
            var netProfitThisMonth = monthPayments - expensesThisMonth;

            // ── Pending mark-edit requests ─────────────────────────────────────
            var pendingMarkEditRequests = await dbContext.MarkEditRequests
                .AsNoTracking()
                .CountAsync(r => r.Status == MarkEditRequestStatus.Pending);

            // ── Active term class count ────────────────────────────────────────
            int activeTermClasses = 0;
            if (activeTerm != null)
            {
                activeTermClasses = await dbContext.CurrentClasses
                    .AsNoTracking()
                    .CountAsync(cc => cc.TermID == activeTerm.TermID && cc.IsActive);
            }

            // ── Recent payments (last 5) ──────────────────────────────────────
            var recentPayments = await dbContext.Payments
                .AsNoTracking()
                .Include(p => p.Student)
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .Select(p => new
                {
                    p.PaymentId,
                    p.Student.StudentID,
                    p.Student.StudentName,
                    p.Student.RegistrationNo,
                    p.TotalAmount,
                    p.PaymentDate,
                    PaymentMethod = p.PaymentMethod.ToString()
                })
                .ToListAsync();

            // ── Pending notifications ──────────────────────────────────────────
            var pendingNotifications = await dbContext.Notifications
                .AsNoTracking()
                .CountAsync(n => n.Status == NotificationStatus.Pending);

            return Ok(new
            {
                GeneratedAt = now,

                Students = new
                {
                    Total    = totalStudents,
                    Enrolled = enrolledStudents
                },

                ActiveTerm = activeTerm == null ? null : new
                {
                    activeTerm.TermID,
                    activeTerm.TermName,
                    ActiveClasses = activeTermClasses
                },

                Fees = new
                {
                    CollectedToday   = todayPayments,
                    CollectedThisMonth = monthPayments,
                    DefaulterCount   = defaulterCount,
                    OutstandingBalance = outstandingBalance,
                    CollectionRate   = collectionRate,
                    TotalBilled      = totalBilled,
                    TotalCollected   = totalCollected
                },

                Finance = new
                {
                    ExpensesThisMonth = expensesThisMonth,
                    NetProfitThisMonth = netProfitThisMonth
                },

                AttendanceToday = new
                {
                    StudentsPresent = presentToday,
                    StudentsAbsent  = absentToday,
                    TotalMarked     = totalMarkedToday,
                    AttendancePct   = attendancePctToday,
                    TeacherBreakdown = teacherAttendanceToday
                },

                Pending = new
                {
                    MarkEditRequests  = pendingMarkEditRequests,
                    Notifications     = pendingNotifications
                },

                RecentPayments = recentPayments
            });
        }
    }
}
