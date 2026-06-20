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
    /// A student enters their Registration No and sees ONLY their own published
    /// (approved) result — name, class, term, monthly/terminal marks, grade and
    /// attendance. No fees, contact details, address, DOB or photo are returned.
    ///
    /// Privacy note: lookup is by Registration No only (product decision). To make
    /// it harder to view someone else's result, pass an optional &dob=yyyy-MM-dd —
    /// if supplied it must match. The frontend can start requiring it later without
    /// any change to this endpoint.
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

        // GET api/public/result?registrationNo=ABC123[&dob=2008-05-01]
        [HttpGet("result")]
        public async Task<IActionResult> GetResult(
            [FromQuery] string registrationNo,
            [FromQuery] DateTime? dob = null)
        {
            if (string.IsNullOrWhiteSpace(registrationNo))
                return BadRequest(new { message = "Please enter your registration number." });

            var reg = registrationNo.Trim();

            var student = await db.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.RegistrationNo == reg);

            // Same generic message whether the student doesn't exist or the second
            // factor mismatches — avoids confirming which reg numbers are valid.
            if (student == null)
                return NotFound(new { message = "No student found with that registration number. Please check and try again." });

            if (dob.HasValue && student.DateOfBirth.Date != dob.Value.Date)
                return NotFound(new { message = "The details entered do not match our records." });

            // ── Which (term, class) results does this student have? ───────────
            var monthlyKeys = await db.StudentMonthlyResults
                .AsNoTracking()
                .Where(r => r.StudentID == student.StudentID)
                .Select(r => new { r.TermID, r.CurrentClassID, r.CreatedOn })
                .ToListAsync();

            var terminalKeys = await db.TerminalResults
                .AsNoTracking()
                .Where(r => r.StudentID == student.StudentID)
                .Select(r => new { r.TermID, r.CurrentClassID })
                .ToListAsync();

            var keys = monthlyKeys
                .Select(k => (k.TermID, k.CurrentClassID))
                .Concat(terminalKeys.Select(k => (k.TermID, k.CurrentClassID)))
                .Distinct()
                .ToList();

            if (keys.Count == 0)
                return NotFound(new { message = "No results have been published for you yet. Please check back later." });

            // ── Keep only APPROVED (published) term/class combinations ─────────
            var approved = await db.ResultApprovals
                .AsNoTracking()
                .Where(a => a.IsApproved)
                .Select(a => new { a.TermID, a.CurrentClassID })
                .ToListAsync();
            var approvedSet = approved.Select(a => (a.TermID, a.CurrentClassID)).ToHashSet();

            var approvedKeys = keys.Where(k => approvedSet.Contains(k)).ToList();
            if (approvedKeys.Count == 0)
                return NotFound(new { message = "Your result has not been published yet. Please check back later." });

            // ── Pick the most recent published term/class for this student ─────
            var recency = monthlyKeys
                .GroupBy(k => (k.TermID, k.CurrentClassID))
                .ToDictionary(g => g.Key, g => g.Max(x => x.CreatedOn));

            var chosen = approvedKeys
                .OrderByDescending(k => recency.TryGetValue(k, out var d) ? d : DateTime.MinValue)
                .First();
            var termId = chosen.Item1;
            var currentClassId = chosen.Item2;

            // ── Build the card (mirrors StudentResultCardController) ────────────
            var gradeCriteria = await db.GradeCriterias
                .AsNoTracking()
                .OrderBy(g => g.DisplayOrder)
                .ToListAsync();

            var term = await db.Term.AsNoTracking().FirstOrDefaultAsync(t => t.TermID == termId);
            var currentClass = await db.CurrentClasses
                .AsNoTracking()
                .Include(cc => cc.Class)
                .Include(cc => cc.Section)
                .FirstOrDefaultAsync(cc => cc.CurrentClassID == currentClassId);

            var termMonths = await db.TermMonths.AsNoTracking().OrderBy(m => m.TermMonth).ToListAsync();

            var monthlyResults = await db.StudentMonthlyResults
                .AsNoTracking()
                .Where(r => r.StudentID == student.StudentID && r.TermID == termId && r.CurrentClassID == currentClassId)
                .ToListAsync();
            var monthlyLookup = monthlyResults.ToDictionary(r => r.TermMonthID);

            var monthlyPassing = await db.TermMonthPassingMarks
                .AsNoTracking()
                .Where(p => p.TermID == termId && p.CurrentClassID == currentClassId)
                .ToDictionaryAsync(p => p.TermMonthID, p => p.PassingMarks);

            var terminal = await db.TerminalResults
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.StudentID == student.StudentID && r.TermID == termId && r.CurrentClassID == currentClassId);

            var terminalPassing = await db.TerminalPassingMarks
                .AsNoTracking()
                .Where(p => p.TermID == termId && p.CurrentClassID == currentClassId)
                .Select(p => (float?)p.PassingMarks)
                .FirstOrDefaultAsync() ?? 0f;

            var attendanceRecords = await db.StudentAttendances
                .AsNoTracking()
                .Where(a => a.StudentID == student.StudentID && a.CurrentClassID == currentClassId)
                .Select(a => new { a.Status })
                .ToListAsync();
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
            }).ToList();

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
