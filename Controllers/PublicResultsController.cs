using InstituteWebAPI.Data;
using InstituteWebAPI.Helpers;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Controllers
{
    /// <summary>
    /// PUBLIC, anonymous result checker for the website (rozhn.org/results).
    /// A student enters their Registration No and sees their own result for the
    /// CURRENT (active) term — name, class, term, monthly/terminal marks, grade and
    /// attendance. No fees, contact, address, DOB or photo are returned.
    ///
    /// Visibility: a result is shown unless an admin has EXPLICITLY un-approved that
    /// term/class in Result Approvals (IsApproved = false). Combos with no approval
    /// record, or an approved record, are shown. Pass ?debug=1 for diagnostics.
    /// Privacy: lookup is by Registration No only (product decision); optional
    /// &dob=yyyy-MM-dd is enforced if supplied.
    /// </summary>
    [Route("api/public")]
    [ApiController]
    [AllowAnonymous]
    public class PublicResultsController : ControllerBase
    {
        private readonly RozhnInstituteDbContext db;

        public PublicResultsController(RozhnInstituteDbContext db)
        {
            this.db = db;
        }

        // GET api/public/result?registrationNo=ABC123[&dob=2008-05-01][&debug=1]
        [HttpGet("result")]
        public async Task<IActionResult> GetResult(
            [FromQuery] string registrationNo,
            [FromQuery] DateTime? dob = null,
            [FromQuery] int debug = 0)
        {
            if (string.IsNullOrWhiteSpace(registrationNo))
                return BadRequest(new { message = "Please enter your registration number." });

            var reg = registrationNo.Trim();

            var student = await db.Students.AsNoTracking()
                .FirstOrDefaultAsync(s => s.RegistrationNo == reg);

            if (student == null)
                return NotFound(new { message = "No student found with that registration number. Please check and try again." });

            if (dob.HasValue && student.DateOfBirth.Date != dob.Value.Date)
                return NotFound(new { message = "The details entered do not match our records." });

            // Active / current term (fall back to the latest term).
            var activeTerm = await db.Term.AsNoTracking().Where(t => t.IsActive)
                                 .OrderByDescending(t => t.TermStart).FirstOrDefaultAsync()
                             ?? await db.Term.AsNoTracking()
                                 .OrderByDescending(t => t.TermStart).FirstOrDefaultAsync();

            // All (term, class) combos this student has result data for.
            var monthlyKeys = await db.StudentMonthlyResults.AsNoTracking()
                .Where(r => r.StudentID == student.StudentID)
                .Select(r => new { r.TermID, r.CurrentClassID, r.CreatedOn })
                .ToListAsync();

            var terminalKeys = await db.TerminalResults.AsNoTracking()
                .Where(r => r.StudentID == student.StudentID)
                .Select(r => new { r.TermID, r.CurrentClassID })
                .ToListAsync();

            var keys = monthlyKeys.Select(k => (k.TermID, k.CurrentClassID))
                .Concat(terminalKeys.Select(k => (k.TermID, k.CurrentClassID)))
                .Distinct().ToList();

            // Approvals: only an EXPLICIT IsApproved == false hides a result.
            var approvals = await db.ResultApprovals.AsNoTracking()
                .Select(a => new { a.TermID, a.CurrentClassID, a.IsApproved })
                .ToListAsync();
            var explicitlyHeld = approvals.Where(a => !a.IsApproved)
                .Select(a => (a.TermID, a.CurrentClassID)).ToHashSet();

            var visibleKeys = keys.Where(k => !explicitlyHeld.Contains(k)).ToList();

            if (debug == 1)
            {
                return Ok(new
                {
                    studentFound = true,
                    student.RegistrationNo,
                    student.StudentName,
                    activeTerm = activeTerm?.TermName,
                    activeTermId = activeTerm?.TermID,
                    monthlyRowCount = monthlyKeys.Count,
                    terminalRowCount = terminalKeys.Count,
                    distinctCombos = keys.Select(k => new { termId = k.Item1, currentClassId = k.Item2 }),
                    approvalRecords = approvals,
                    explicitlyHeldCount = explicitlyHeld.Count,
                    visibleComboCount = visibleKeys.Count,
                });
            }

            if (keys.Count == 0)
                return NotFound(new { message = "No results have been entered for you yet. Please check back later." });

            if (visibleKeys.Count == 0)
                return NotFound(new { message = "Your result has not been published yet. Please check back later." });

            // Choose the combo: prefer the active term, then the most recently updated.
            var recency = monthlyKeys.GroupBy(k => (k.TermID, k.CurrentClassID))
                .ToDictionary(g => g.Key, g => g.Max(x => x.CreatedOn));

            var chosen = visibleKeys
                .OrderByDescending(k => activeTerm != null && k.Item1 == activeTerm.TermID)
                .ThenByDescending(k => recency.TryGetValue(k, out var d) ? d : DateTime.MinValue)
                .First();
            var termId = chosen.Item1;
            var currentClassId = chosen.Item2;

            // ── Build the detailed card (mirrors StudentResultCardController) ──
            var gradeCriteria = await db.GradeCriterias.AsNoTracking()
                .OrderBy(g => g.DisplayOrder).ToListAsync();

            var term = await db.Term.AsNoTracking().FirstOrDefaultAsync(t => t.TermID == termId);
            var currentClass = await db.CurrentClasses.AsNoTracking()
                .Include(cc => cc.Class)
                .Include(cc => cc.Section)
                .FirstOrDefaultAsync(cc => cc.CurrentClassID == currentClassId);

            var termMonths = await db.TermMonths.AsNoTracking().OrderBy(m => m.TermMonth).ToListAsync();

            var monthlyResults = await db.StudentMonthlyResults.AsNoTracking()
                .Where(r => r.StudentID == student.StudentID && r.TermID == termId && r.CurrentClassID == currentClassId)
                .ToListAsync();
            var monthlyLookup = monthlyResults.ToDictionary(r => r.TermMonthID);

            var monthlyPassing = await db.TermMonthPassingMarks.AsNoTracking()
                .Where(p => p.TermID == termId && p.CurrentClassID == currentClassId)
                .ToDictionaryAsync(p => p.TermMonthID, p => p.PassingMarks);

            var terminal = await db.TerminalResults.AsNoTracking()
                .FirstOrDefaultAsync(r => r.StudentID == student.StudentID && r.TermID == termId && r.CurrentClassID == currentClassId);

            var terminalPassing = await db.TerminalPassingMarks.AsNoTracking()
                .Where(p => p.TermID == termId && p.CurrentClassID == currentClassId)
                .Select(p => (float?)p.PassingMarks).FirstOrDefaultAsync() ?? 0f;

            var attendanceRecords = await db.StudentAttendances.AsNoTracking()
                .Where(a => a.StudentID == student.StudentID && a.CurrentClassID == currentClassId)
                .Select(a => new { a.Status }).ToListAsync();
            var totalDays = attendanceRecords.Count;
            var presentDays = attendanceRecords.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late);
            var absentDays = attendanceRecords.Count(a => a.Status == AttendanceStatus.Absent);
            var attendancePct = totalDays > 0 ? Math.Round((double)presentDays / totalDays * 100, 1) : 0.0;

            var monthRows = termMonths.Select(tm =>
            {
                monthlyLookup.TryGetValue(tm.TermMonthID, out var res);
                monthlyPassing.TryGetValue(tm.TermMonthID, out var passing);

                var obtained = res?.ObtainedMarks ?? 0f;
                var total = res?.TotalMarks ?? 0f;
                var pct = res?.Percentage ?? 0f;
                var status = res?.Status;
                var hasStatus = !string.IsNullOrWhiteSpace(status);
                var grade = (!hasStatus && total > 0) ? GradeCalculator.Resolve(pct, gradeCriteria) : "-";
                var pass = total > 0 && obtained >= passing;

                return new
                {
                    MonthNumber = tm.TermMonth,
                    TotalMarks = total,
                    ObtainedMarks = obtained,
                    Percentage = Math.Round(pct, 1),
                    PassingMarks = passing,
                    Grade = grade,
                    Result = hasStatus ? status! : (total > 0 ? (pass ? "Pass" : "Fail") : "N/A")
                };
            }).Where(m => m.TotalMarks > 0 || m.Result != "N/A").ToList();

            object? terminalRow = null;
            if (terminal != null)
            {
                terminalRow = new
                {
                    TotalMarksConsidered = terminal.TotalMarksConsidered,
                    TotalObtained = terminal.TotalObtained,
                    Percentage = Math.Round(terminal.Percentage, 1),
                    PassingMarks = terminalPassing,
                    Grade = GradeCalculator.Resolve(terminal.Percentage, gradeCriteria),
                    Result = terminal.Result
                };
            }

            var overallResult = terminal?.Result ?? "Pending";
            var overallPct = terminal != null ? (float)Math.Round(terminal.Percentage, 1) : 0f;
            var overallGrade = terminal != null ? GradeCalculator.Resolve(terminal.Percentage, gradeCriteria) : "-";

            return Ok(new
            {
                Student = new
                {
                    student.RegistrationNo,
                    student.StudentName,
                    student.FatherName
                },
                Term = new { TermName = term?.TermName },
                Class = new
                {
                    ClassName = currentClass?.Class?.ClassName,
                    SectionName = currentClass?.Section?.Name
                },
                MonthlyResults = monthRows,
                TerminalResult = terminalRow,
                Attendance = new
                {
                    TotalDays = totalDays,
                    PresentDays = presentDays,
                    AbsentDays = absentDays,
                    AttendancePct = attendancePct
                },
                Overall = new
                {
                    Result = overallResult,
                    Percentage = overallPct,
                    Grade = overallGrade
                }
            });
        }
    }
}
