using InstituteWebAPI.Data;
using InstituteWebAPI.Helpers;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Controllers
{
    /// <summary>
    /// Produces a print-ready result card for one student in one term.
    /// Combines monthly results, terminal result, passing marks, grades,
    /// and attendance — everything the frontend needs to render the card.
    /// </summary>
    [Route("api/students/{studentId:Guid}/result-card")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher")]
    public class StudentResultCardController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;

        public StudentResultCardController(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // Grade resolution is now delegated to GradeCalculator.Resolve(), which
        // uses DB-configured GradeCriteria loaded once per request (see GetResultCard).
        // The hardcoded switch was removed because it silently ignored admin-configured
        // grade boundaries.

        // ── GET api/students/{studentId}/result-card?termId=&currentClassId= ──
        [HttpGet]
        public async Task<IActionResult> GetResultCard(
            [FromRoute] Guid studentId,
            [FromQuery] Guid termId,
            [FromQuery] Guid currentClassId)
        {
            if (studentId      == Guid.Empty) return BadRequest("studentId is required.");
            if (termId         == Guid.Empty) return BadRequest("termId is required.");
            if (currentClassId == Guid.Empty) return BadRequest("currentClassId is required.");

            // ── Grade criteria (loaded once per request) ──────────────────────
            // Loaded from the DB so admin-configured thresholds are respected.
            // Falls back to built-in defaults (A+=80…F) when the table is empty.
            var gradeCriteria = await dbContext.GradeCriterias
                .AsNoTracking()
                .OrderBy(g => g.DisplayOrder)
                .ToListAsync();

            // ── Student ───────────────────────────────────────────────────────
            var student = await dbContext.Students
                .AsNoTracking()
                .Include(s => s.Village)
                .FirstOrDefaultAsync(s => s.StudentID == studentId);

            if (student == null) return NotFound("Student not found.");

            // ── Term + class ──────────────────────────────────────────────────
            var term = await dbContext.Term
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TermID == termId);
            if (term == null) return NotFound("Term not found.");

            var currentClass = await dbContext.CurrentClasses
                .AsNoTracking()
                .Include(cc => cc.Class)
                .Include(cc => cc.Teacher)
                .Include(cc => cc.Slot)
                .Include(cc => cc.Section)
                .FirstOrDefaultAsync(cc => cc.CurrentClassID == currentClassId);
            if (currentClass == null) return NotFound("Class not found.");

            // ── Term months (ordered by TermMonth int) ────────────────────────
            var termMonths = await dbContext.TermMonths
                .AsNoTracking()
                .OrderBy(m => m.TermMonth)
                .ToListAsync();

            // ── Monthly results ───────────────────────────────────────────────
            var monthlyResults = await dbContext.StudentMonthlyResults
                .AsNoTracking()
                .Where(r =>
                    r.StudentID      == studentId &&
                    r.TermID         == termId    &&
                    r.CurrentClassID == currentClassId)
                .ToListAsync();

            var monthlyLookup = monthlyResults.ToDictionary(r => r.TermMonthID);

            // ── Monthly passing marks ─────────────────────────────────────────
            var monthlyPassing = await dbContext.TermMonthPassingMarks
                .AsNoTracking()
                .Where(p => p.TermID == termId && p.CurrentClassID == currentClassId)
                .ToDictionaryAsync(p => p.TermMonthID, p => p.PassingMarks);

            // ── Terminal result ───────────────────────────────────────────────
            var terminal = await dbContext.TerminalResults
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.StudentID      == studentId &&
                    r.TermID         == termId    &&
                    r.CurrentClassID == currentClassId);

            // ── Terminal passing mark ─────────────────────────────────────────
            var terminalPassing = await dbContext.TerminalPassingMarks
                .AsNoTracking()
                .Where(p => p.TermID == termId && p.CurrentClassID == currentClassId)
                .Select(p => (float?)p.PassingMarks)
                .FirstOrDefaultAsync() ?? 0f;

            // ── Attendance for the term's months ──────────────────────────────
            // Use the first day of the term's first month to the last day of the last month
            // as the attendance window — approximate by calendar year if needed.
            var attendanceRecords = await dbContext.StudentAttendances
                .AsNoTracking()
                .Where(a =>
                    a.StudentID      == studentId &&
                    a.CurrentClassID == currentClassId)
                .Select(a => new { a.AttendanceDate, a.Status })
                .ToListAsync();

            var totalDays   = attendanceRecords.Count;
            var presentDays = attendanceRecords.Count(a =>
                a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late);
            var absentDays  = attendanceRecords.Count(a => a.Status == AttendanceStatus.Absent);
            var attendancePct = totalDays > 0
                ? Math.Round((double)presentDays / totalDays * 100, 1)
                : 0.0;

            // ── Shape monthly rows ────────────────────────────────────────────
            var monthRows = termMonths.Select(tm =>
            {
                monthlyLookup.TryGetValue(tm.TermMonthID, out var res);
                monthlyPassing.TryGetValue(tm.TermMonthID, out var passing);

                var obtained = res?.ObtainedMarks ?? 0f;
                var total    = res?.TotalMarks    ?? 0f;
                var pct      = res?.Percentage    ?? 0f;
                var status   = res?.Status;
                var hasStatus = !string.IsNullOrWhiteSpace(status);
                var grade    = (!hasStatus && total > 0) ? GradeCalculator.Resolve(pct, gradeCriteria) : "-";
                var pass     = total > 0 && obtained >= passing;

                return new
                {
                    TermMonthID    = tm.TermMonthID,
                    MonthNumber    = tm.TermMonth,
                    TotalMarks     = total,
                    ObtainedMarks  = obtained,
                    Percentage     = Math.Round(pct, 1),
                    PassingMarks   = passing,
                    Grade          = grade,
                    // A status note (e.g. "Not Conducted") overrides Pass/Fail for the month.
                    Result         = hasStatus ? status! : (total > 0 ? (pass ? "Pass" : "Fail") : "N/A")
                };
            }).ToList();

            // ── Shape terminal row ────────────────────────────────────────────
            object? terminalRow = null;
            if (terminal != null)
            {
                var tPct   = terminal.Percentage;
                var tGrade = GradeCalculator.Resolve(tPct, gradeCriteria);
                terminalRow = new
                {
                    TotalMarksConsidered = terminal.TotalMarksConsidered,
                    TotalObtained        = terminal.TotalObtained,
                    Percentage           = Math.Round(tPct, 1),
                    PassingMarks         = terminalPassing,
                    Grade                = tGrade,
                    Result               = terminal.Result,
                    IncludeMonth1        = terminal.IncludeMonth1,
                    IncludeMonth2        = terminal.IncludeMonth2
                };
            }

            // ── Overall result ────────────────────────────────────────────────
            // A student passes overall only if terminal result exists and is Pass.
            var overallResult = terminal?.Result ?? "Pending";
            var overallPct    = terminal != null
                ? (float)Math.Round(terminal.Percentage, 1)
                : 0f;
            var overallGrade  = terminal != null ? GradeCalculator.Resolve(terminal.Percentage, gradeCriteria) : "-";

            return Ok(new
            {
                // ── Header info ────────────────────────────────────────────
                GeneratedAt = DateTime.UtcNow,

                Student = new
                {
                    student.StudentID,
                    student.RegistrationNo,
                    student.StudentName,
                    student.FatherName,
                    student.DateOfBirth,
                    Village = student.Village?.VillageName,
                    student.Gender,
                    student.Picture
                },

                Term = new
                {
                    term.TermID,
                    term.TermName
                },

                Class = new
                {
                    currentClass.CurrentClassID,
                    ClassName   = currentClass.Class?.ClassName,
                    TeacherName = currentClass.Teacher?.TeacherName,
                    SlotName    = currentClass.Slot?.SlotName,
                    SectionName = currentClass.Section?.Name
                },

                // ── Monthly results ────────────────────────────────────────
                MonthlyResults = monthRows,

                // ── Terminal result ────────────────────────────────────────
                TerminalResult = terminalRow,

                // ── Attendance ─────────────────────────────────────────────
                Attendance = new
                {
                    TotalDays     = totalDays,
                    PresentDays   = presentDays,
                    AbsentDays    = absentDays,
                    AttendancePct = attendancePct
                },

                // ── Overall ────────────────────────────────────────────────
                Overall = new
                {
                    Result        = overallResult,
                    Percentage    = overallPct,
                    Grade         = overallGrade
                }
            });
        }
    }
}
