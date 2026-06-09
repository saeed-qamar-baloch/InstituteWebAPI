using InstituteWebAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class StudentTimelineController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;

        public StudentTimelineController(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // Display label for an enrolment: the active-term class shows "Enrolled";
        // past-term classes show the outcome from that term's terminal result.
        private static string EnrolmentDisplayStatus(bool isActiveTerm, string? rawStatus, string? terminalResult)
        {
            if (isActiveTerm) return "Enrolled";

            if (!string.IsNullOrWhiteSpace(terminalResult))
            {
                if (terminalResult.Equals("Fail", StringComparison.OrdinalIgnoreCase)) return "Failed";
                if (terminalResult.Equals("Promoted", StringComparison.OrdinalIgnoreCase)) return "Promoted";
                // Pass / 1st / 2nd / 3rd → passed the term.
                return "Passed";
            }

            // No terminal result on record — fall back to the stored enrolment status.
            return string.IsNullOrWhiteSpace(rawStatus) ? "—" : rawStatus!;
        }

        // ── GET api/StudentTimeline/{studentId} ───────────────────────────────
        // Returns the full history of a student in one call:
        //   • Basic student info
        //   • All admissions (with fee dues per admission)
        //   • Class enrolments (with class/slot/section/teacher labels)
        //   • Monthly results (per term+month)
        //   • Terminal results (per term)
        [HttpGet("{studentId:Guid}")]
        public async Task<IActionResult> GetTimeline([FromRoute] Guid studentId)
        {
            // ── Student info ──────────────────────────────────────────────────
            var student = await dbContext.Students
                .AsNoTracking()
                .Include(s => s.Village)
                .FirstOrDefaultAsync(s => s.StudentID == studentId);

            if (student == null) return NotFound("Student not found.");

            // ── Admissions + fee dues ─────────────────────────────────────────
            var admissions = await dbContext.Admissions
                .AsNoTracking()
                .Include(a => a.Course)
                .Include(a => a.AdmittedClass)
                .Include(a => a.FeeDues)
                    .ThenInclude(d => d.PaymentDetails)
                .Where(a => a.StudentID == studentId)
                .OrderBy(a => a.RegistrationDate)
                .ToListAsync();

            // ── Class enrolments ──────────────────────────────────────────────
            var enrolments = await dbContext.ClassStudents
                .AsNoTracking()
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Class)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Slot)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc!.Section)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Teacher)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Term)
                .Where(cs => cs.StudentID == studentId)
                .ToListAsync();

            // ── Monthly results ───────────────────────────────────────────────
            var monthlyResults = await dbContext.StudentMonthlyResults
                .AsNoTracking()
                .Include(r => r.Term)
                .Include(r => r.TermMonth)
                .Include(r => r.CurrentClass)
                    .ThenInclude(cc => cc.Class)
                .Where(r => r.StudentID == studentId)
                .OrderBy(r => r.Term.TermName)
                .ThenBy(r => r.TermMonth.TermMonth)
                .ToListAsync();

            // ── Terminal results ──────────────────────────────────────────────
            var terminalResults = await dbContext.TerminalResults
                .AsNoTracking()
                .Include(r => r.Term)
                .Include(r => r.CurrentClass)
                    .ThenInclude(cc => cc.Class)
                .Where(r => r.StudentID == studentId)
                .OrderBy(r => r.Term.TermName)
                .ToListAsync();

            // ── Scholarships ──────────────────────────────────────────────────
            var scholarships = await dbContext.Scholarships
                .AsNoTracking()
                .Where(s => s.StudentID == studentId)
                .OrderBy(s => s.FromMonth)
                .ToListAsync();

            // ── Leave requests ────────────────────────────────────────────────
            var leaveRequests = await dbContext.StudentLeaveRequests
                .AsNoTracking()
                .Include(r => r.CurrentClass).ThenInclude(cc => cc.Class)
                .Where(r => r.StudentID == studentId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Terminal result per class (for non-active-term enrolment outcomes).
            var terminalByClass = terminalResults
                .GroupBy(r => r.CurrentClassID)
                .ToDictionary(g => g.Key, g => g.First().Result);

            // ── Shape the response ────────────────────────────────────────────
            return Ok(new
            {
                Student = new
                {
                    student.StudentID,
                    student.RegistrationNo,
                    student.StudentName,
                    student.FatherName,
                    student.Gender,
                    student.DateOfBirth,
                    Village    = student.Village?.VillageName,
                    student.City,
                    student.FatherContact,
                    student.StudentContact,
                    student.IsEnrolled,
                    student.Remarks
                },

                Admissions = admissions.Select(a => new
                {
                    a.AdmissionID,
                    a.RegistrationDate,
                    Course        = a.Course?.CourseName,
                    AdmittedClass = a.AdmittedClass?.ClassName,
                    a.MonthlyFee,
                    a.Status,
                    a.IsActive,
                    a.LeavingDate,
                    FeeDues = a.FeeDues.Select(d => new
                    {
                        d.FeeDueId,
                        FeeType    = d.FeeType.ToString(),
                        d.FeeMonth,
                        d.BaseAmount,
                        d.LateFeeAmount,
                        d.IsLateFeeWaived,
                        TotalAmount   = d.BaseAmount + (d.IsLateFeeWaived ? 0m : d.LateFeeAmount),
                        PaidAmount    = d.PaymentDetails.Sum(p => p.PaidAmount),
                        d.DueDate,
                        Status     = d.Status.ToString()
                    }).OrderBy(d => d.FeeMonth).ThenBy(d => d.FeeType)
                }),

                Enrolments = enrolments.Select(cs => new
                {
                    cs.ClassStudentID,
                    cs.CurrentClassID,
                    ClassName   = cs.CurrentClass?.Class?.ClassName,
                    SlotName    = cs.CurrentClass?.Slot?.SlotName,
                    SectionName = cs.CurrentClass?.Section?.Name,
                    TeacherName = cs.CurrentClass?.Teacher?.TeacherName,
                    TermName    = cs.CurrentClass?.Term?.TermName,
                    TermStart   = cs.CurrentClass?.Term?.TermStart,
                    CreatedOn   = cs.CurrentClass != null ? cs.CurrentClass.CreatedOn : (DateTime?)null,
                    // Active-term class → "Enrolled"; past terms → Passed/Failed/Promoted.
                    Status      = EnrolmentDisplayStatus(
                                      cs.CurrentClass?.Term?.IsActive ?? false,
                                      cs.Status,
                                      terminalByClass.TryGetValue(cs.CurrentClassID, out var trTimeline) ? trTimeline : null),
                    RawStatus   = cs.Status
                }),

                MonthlyResults = monthlyResults.Select(r => new
                {
                    r.StudentMonthlyResultID,
                    TermName      = r.Term?.TermName,
                    Month         = r.TermMonth?.TermMonth,
                    ClassName     = r.CurrentClass?.Class?.ClassName,
                    r.CurrentClassID,
                    r.TotalMarks,
                    r.ObtainedMarks,
                    r.Percentage
                }),

                TerminalResults = terminalResults.Select(r => new
                {
                    r.TerminalResultID,
                    TermName    = r.Term?.TermName,
                    TermStart   = r.Term?.TermStart,
                    TermEnd     = r.Term?.TermEnd,
                    ClassName   = r.CurrentClass?.Class?.ClassName,
                    r.CurrentClassID,
                    r.TotalMarksConsidered,
                    r.TotalObtained,
                    r.Percentage,
                    r.Result,
                    r.IncludeMonth1,
                    r.IncludeMonth2
                }),

                Scholarships = scholarships.Select(s => new
                {
                    s.ScholarshipID,
                    s.DiscountPercent,
                    s.FromMonth,
                    s.ToMonth,
                    Status = s.Status.ToString(),
                    s.Reason
                }),

                LeaveRequests = leaveRequests.Select(r => new
                {
                    r.StudentLeaveRequestID,
                    r.LeavingDate,
                    r.Reason,
                    Status    = r.Status.ToString(),
                    r.ReviewRemarks,
                    r.CreatedAt,
                    ClassName = r.CurrentClass?.Class?.ClassName
                })
            });
        }

        // ── GET api/StudentTimeline/{studentId}/fees ──────────────────────────
        // Lightweight fee-only view — useful for the cashier screen.
        [HttpGet("{studentId:Guid}/fees")]
        public async Task<IActionResult> GetFeeHistory([FromRoute] Guid studentId)
        {
            var student = await dbContext.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudentID == studentId);

            if (student == null) return NotFound("Student not found.");

            var admissions = await dbContext.Admissions
                .AsNoTracking()
                .Include(a => a.Course)
                .Include(a => a.FeeDues)
                    .ThenInclude(d => d.PaymentDetails)
                .Where(a => a.StudentID == studentId)
                .OrderByDescending(a => a.IsActive)
                .ThenBy(a => a.RegistrationDate)
                .ToListAsync();

            var totalDue  = admissions.SelectMany(a => a.FeeDues)
                .Sum(d => d.BaseAmount + (d.IsLateFeeWaived ? 0m : d.LateFeeAmount));
            var totalPaid = admissions.SelectMany(a => a.FeeDues)
                .SelectMany(d => d.PaymentDetails)
                .Sum(p => p.PaidAmount);

            return Ok(new
            {
                StudentID      = studentId,
                StudentName    = student.StudentName,
                RegistrationNo = student.RegistrationNo,
                TotalDue       = totalDue,
                TotalPaid      = totalPaid,
                Balance        = totalDue - totalPaid,
                Admissions = admissions.Select(a => new
                {
                    a.AdmissionID,
                    Course     = a.Course?.CourseName,
                    a.IsActive,
                    a.Status,
                    FeeDues    = a.FeeDues.Select(d => new
                    {
                        d.FeeDueId,
                        FeeType      = d.FeeType.ToString(),
                        d.FeeMonth,
                        d.BaseAmount,
                        d.LateFeeAmount,
                        d.IsLateFeeWaived,
                        TotalAmount  = d.BaseAmount + (d.IsLateFeeWaived ? 0m : d.LateFeeAmount),
                        PaidAmount   = d.PaymentDetails.Sum(p => p.PaidAmount),
                        d.DueDate,
                        Status       = d.Status.ToString(),
                        Payments     = d.PaymentDetails.Select(p => new
                        {
                            p.PaymentDetailId,
                            p.PaidAmount
                        })
                    }).OrderBy(d => d.FeeMonth).ThenBy(d => d.FeeType)
                })
            });
        }

        // ── GET api/StudentTimeline/{studentId}/academic ──────────────────────
        // Academic-only view: enrolments + monthly + terminal results.
        [HttpGet("{studentId:Guid}/academic")]
        public async Task<IActionResult> GetAcademicHistory([FromRoute] Guid studentId)
        {
            var student = await dbContext.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudentID == studentId);

            if (student == null) return NotFound("Student not found.");

            var enrolments = await dbContext.ClassStudents
                .AsNoTracking()
                .Include(cs => cs.CurrentClass).ThenInclude(cc => cc.Class)
                .Include(cs => cs.CurrentClass).ThenInclude(cc => cc.Term)
                .Where(cs => cs.StudentID == studentId)
                .ToListAsync();

            var monthlyResults = await dbContext.StudentMonthlyResults
                .AsNoTracking()
                .Include(r => r.Term)
                .Include(r => r.TermMonth)
                .Include(r => r.CurrentClass).ThenInclude(cc => cc.Class)
                .Where(r => r.StudentID == studentId)
                .OrderBy(r => r.Term.TermName).ThenBy(r => r.TermMonth.TermMonth)
                .ToListAsync();

            var terminalResults = await dbContext.TerminalResults
                .AsNoTracking()
                .Include(r => r.Term)
                .Include(r => r.CurrentClass).ThenInclude(cc => cc.Class)
                .Where(r => r.StudentID == studentId)
                .OrderBy(r => r.Term.TermName)
                .ToListAsync();

            var terminalByClass = terminalResults
                .GroupBy(r => r.CurrentClassID)
                .ToDictionary(g => g.Key, g => g.First().Result);

            return Ok(new
            {
                StudentID      = studentId,
                StudentName    = student.StudentName,
                RegistrationNo = student.RegistrationNo,

                Enrolments = enrolments.Select(cs => new
                {
                    cs.ClassStudentID,
                    ClassName   = cs.CurrentClass?.Class?.ClassName,
                    TermName    = cs.CurrentClass?.Term?.TermName,
                    Status      = EnrolmentDisplayStatus(
                                      cs.CurrentClass?.Term?.IsActive ?? false,
                                      cs.Status,
                                      terminalByClass.TryGetValue(cs.CurrentClassID, out var trAcademic) ? trAcademic : null),
                    RawStatus   = cs.Status
                }),

                MonthlyResults = monthlyResults.Select(r => new
                {
                    TermName    = r.Term?.TermName,
                    Month       = r.TermMonth?.TermMonth,
                    ClassName   = r.CurrentClass?.Class?.ClassName,
                    r.TotalMarks,
                    r.ObtainedMarks,
                    r.Percentage
                }),

                TerminalResults = terminalResults.Select(r => new
                {
                    TermName    = r.Term?.TermName,
                    ClassName   = r.CurrentClass?.Class?.ClassName,
                    r.TotalMarksConsidered,
                    r.TotalObtained,
                    r.Percentage,
                    r.Result
                })
            });
        }
    }
}
