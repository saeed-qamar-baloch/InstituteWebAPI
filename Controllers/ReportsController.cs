using InstituteWebAPI.Data;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;

        public ReportsController(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // ── GET api/reports/defaulters ────────────────────────────────────────
        // Students who have at least one Unpaid or Partial fee due whose DueDate
        // is in the past. Optional filters: courseId, classId, fromDate, toDate.
        // Returns one row per student with their total outstanding balance.
        [HttpGet("defaulters")]
        public async Task<IActionResult> GetDefaulters(
            [FromQuery] Guid?     courseId  = null,
            [FromQuery] Guid?     classId   = null,
            [FromQuery] DateTime? fromDate  = null,
            [FromQuery] DateTime? toDate    = null)
        {
            var today = DateTime.UtcNow.Date;

            var query = dbContext.FeeDues
                .AsNoTracking()
                .Include(d => d.Admission)
                    .ThenInclude(a => a.Student)
                .Include(d => d.Admission)
                    .ThenInclude(a => a.Course)
                .Include(d => d.PaymentDetails)
                .Where(d =>
                    d.DueDate < today &&
                    (d.Status == FeeDueStatus.Unpaid || d.Status == FeeDueStatus.Partial) &&
                    d.Admission.IsActive &&
                    // A due with nothing actually owed (waived, or a 0-amount fee) is
                    // never a real default, even if its stored Status hasn't been
                    // self-healed to Waived yet by GetUnpaidDuesAsync. NR ("Not
                    // Registered") admission-month placeholders are excluded explicitly
                    // too — the amount check already covers them, but this keeps intent
                    // clear even if NR dues ever carry a nonzero amount in the future.
                    (d.BaseAmount + (d.IsLateFeeWaived ? 0m : d.LateFeeAmount)) > 0m &&
                    d.Status != FeeDueStatus.NR);

            if (courseId.HasValue)
                query = query.Where(d => d.Admission.CourseID == courseId.Value);

            if (fromDate.HasValue)
                query = query.Where(d => d.DueDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(d => d.DueDate <= toDate.Value.Date);

            var dues = await query.ToListAsync();

            // Also filter by classId via ClassStudents if requested
            HashSet<Guid>? enrolledStudentIds = null;
            if (classId.HasValue)
            {
                var ids = await dbContext.ClassStudents
                    .AsNoTracking()
                    .Where(cs => cs.CurrentClassID == classId.Value && cs.Status == "Enrolled")
                    .Select(cs => cs.StudentID)
                    .ToListAsync();
                enrolledStudentIds = new HashSet<Guid>(ids);
            }

            var grouped = dues
                .GroupBy(d => d.Admission.StudentID)
                .Where(g => enrolledStudentIds == null || enrolledStudentIds.Contains(g.Key))
                .Select(g =>
                {
                    var student    = g.First().Admission.Student;
                    var course     = g.First().Admission.Course;
                    var totalDue   = g.Sum(d => d.BaseAmount + (d.IsLateFeeWaived ? 0m : d.LateFeeAmount));
                    var totalPaid  = g.SelectMany(d => d.PaymentDetails).Sum(p => p.PaidAmount);

                    return new
                    {
                        StudentID      = g.Key,
                        student.StudentName,
                        student.FatherName,
                        student.RegistrationNo,
                        student.FatherContact,
                        CourseName     = course?.CourseName,
                        OverdueDues    = g.Count(),
                        TotalDue       = totalDue,
                        TotalPaid      = totalPaid,
                        OutstandingBalance = totalDue - totalPaid,
                        OldestDueDate  = g.Min(d => d.DueDate),
                        Dues           = g.OrderBy(d => d.DueDate).Select(d => new
                        {
                            d.FeeDueId,
                            FeeType    = d.FeeType.ToString(),
                            d.FeeMonth,
                            d.DueDate,
                            d.BaseAmount,
                            d.LateFeeAmount,
                            d.IsLateFeeWaived,
                            AmountDue  = d.BaseAmount + (d.IsLateFeeWaived ? 0m : d.LateFeeAmount),
                            PaidAmount = d.PaymentDetails.Sum(p => p.PaidAmount),
                            Status     = d.Status.ToString()
                        })
                    };
                })
                .OrderByDescending(x => x.OutstandingBalance)
                .ToList();

            return Ok(new
            {
                GeneratedAt        = DateTime.UtcNow,
                TotalDefaulters    = grouped.Count,
                TotalOutstanding   = grouped.Sum(x => x.OutstandingBalance),
                Defaulters         = grouped
            });
        }

        // ── GET api/reports/revenue ───────────────────────────────────────────
        // Fee collection summary grouped by month.
        // Optional filters: fromDate, toDate, paymentMethod, courseId.
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue(
            [FromQuery] DateTime?      fromDate       = null,
            [FromQuery] DateTime?      toDate         = null,
            [FromQuery] PaymentMethod? paymentMethod  = null,
            [FromQuery] Guid?          courseId       = null)
        {
            var query = dbContext.PaymentDetails
                .AsNoTracking()
                .Include(pd => pd.Payment)
                .Include(pd => pd.FeeDue)
                    .ThenInclude(d => d.Admission)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(pd => pd.Payment.PaymentDate >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(pd => pd.Payment.PaymentDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

            if (paymentMethod.HasValue)
                query = query.Where(pd => pd.Payment.PaymentMethod == paymentMethod.Value);

            if (courseId.HasValue)
                query = query.Where(pd => pd.FeeDue.Admission.CourseID == courseId.Value);

            var details = await query.ToListAsync();

            // Group by calendar month of payment
            var byMonth = details
                .GroupBy(pd => new DateTime(pd.Payment.PaymentDate.Year, pd.Payment.PaymentDate.Month, 1))
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Month            = g.Key.ToString("yyyy-MM"),
                    TotalCollected   = g.Sum(pd => pd.PaidAmount),
                    PaymentCount     = g.Select(pd => pd.PaymentId).Distinct().Count(),
                    ByFeeType        = g
                        .GroupBy(pd => pd.FeeDue.FeeType.ToString())
                        .Select(ft => new
                        {
                            FeeType  = ft.Key,
                            Amount   = ft.Sum(pd => pd.PaidAmount)
                        })
                })
                .ToList();

            // Overall totals
            var grandTotal    = details.Sum(pd => pd.PaidAmount);
            var byFeeTypeTotals = details
                .GroupBy(pd => pd.FeeDue.FeeType.ToString())
                .Select(g => new { FeeType = g.Key, Amount = g.Sum(pd => pd.PaidAmount) });

            return Ok(new
            {
                GeneratedAt    = DateTime.UtcNow,
                FromDate       = fromDate,
                ToDate         = toDate,
                GrandTotal     = grandTotal,
                ByFeeType      = byFeeTypeTotals,
                ByMonth        = byMonth
            });
        }

        // ── GET api/reports/class-results ─────────────────────────────────────
        // Pass/fail summary for every class in a given term.
        // Returns aggregate stats per class plus per-student result rows.
        [HttpGet("class-results")]
        public async Task<IActionResult> GetClassResults([FromQuery] Guid termId)
        {
            if (termId == Guid.Empty) return BadRequest("termId is required.");

            var term = await dbContext.Term.AsNoTracking().FirstOrDefaultAsync(t => t.TermID == termId);
            if (term == null) return NotFound("Term not found.");

            // All terminal results for the term, grouped by class
            var results = await dbContext.TerminalResults
                .AsNoTracking()
                .Include(r => r.Student)
                .Include(r => r.CurrentClass)
                    .ThenInclude(cc => cc.Class)
                .Include(r => r.CurrentClass)
                    .ThenInclude(cc => cc.Teacher)
                .Where(r => r.TermID == termId)
                .ToListAsync();

            var byClass = results
                .GroupBy(r => r.CurrentClassID)
                .Select(g =>
                {
                    var cc       = g.First().CurrentClass;
                    var total    = g.Count();
                    var passed   = g.Count(r => r.Result == "Pass");
                    var failed   = g.Count(r => r.Result == "Fail");
                    var avgPct   = total > 0 ? g.Average(r => r.Percentage) : 0f;

                    return new
                    {
                        CurrentClassID = g.Key,
                        ClassName      = cc?.Class?.ClassName,
                        TeacherName    = cc?.Teacher?.TeacherName,
                        TotalStudents  = total,
                        Passed         = passed,
                        Failed         = failed,
                        PassRate       = total > 0 ? Math.Round((double)passed / total * 100, 1) : 0,
                        AveragePercentage = Math.Round(avgPct, 1),
                        Students       = g.OrderByDescending(r => r.Percentage).Select((r, idx) => new
                        {
                            Position       = idx + 1,
                            r.StudentID,
                            r.Student.StudentName,
                            r.Student.RegistrationNo,
                            r.TotalMarksConsidered,
                            r.TotalObtained,
                            Percentage     = Math.Round(r.Percentage, 1),
                            r.Result,
                            r.IncludeMonth1,
                            r.IncludeMonth2
                        })
                    };
                })
                .OrderBy(c => c.ClassName)
                .ToList();

            return Ok(new
            {
                GeneratedAt  = DateTime.UtcNow,
                TermID       = termId,
                TermName     = term.TermName,
                TotalClasses = byClass.Count,
                Classes      = byClass
            });
        }

        // ── GET api/reports/attendance ────────────────────────────────────────
        // Monthly attendance summary for a class.
        // Returns each enrolled student's Present / Absent / Late / Leave counts
        // for the requested month.  If no month/year supplied, defaults to current month.
        [HttpGet("attendance")]
        public async Task<IActionResult> GetAttendanceSummary(
            [FromQuery] Guid currentClassId,
            [FromQuery] int? year  = null,
            [FromQuery] int? month = null)
        {
            if (currentClassId == Guid.Empty)
                return BadRequest("currentClassId is required.");

            var now       = DateTime.UtcNow;
            var y         = year  ?? now.Year;
            var m         = month ?? now.Month;
            var startDate = new DateTime(y, m, 1);
            var endDate   = startDate.AddMonths(1).AddDays(-1);

            // Enrolled students
            var students = await dbContext.ClassStudents
                .AsNoTracking()
                .Include(cs => cs.Student)
                .Include(cs => cs.CurrentClass).ThenInclude(cc => cc.Class)
                .Where(cs => cs.CurrentClassID == currentClassId && cs.Status == "Enrolled")
                .Select(cs => new
                {
                    cs.StudentID,
                    cs.Student.StudentName,
                    cs.Student.RegistrationNo,
                    ClassName = cs.CurrentClass.Class.ClassName
                })
                .OrderBy(x => x.StudentName)
                .ToListAsync();

            if (!students.Any())
                return Ok(new { Message = "No enrolled students in this class.", Rows = Array.Empty<object>() });

            var studentIds = students.Select(s => s.StudentID).ToList();

            // Attendance records for the month
            var records = await dbContext.StudentAttendances
                .AsNoTracking()
                .Where(a =>
                    a.CurrentClassID == currentClassId &&
                    a.AttendanceDate >= startDate &&
                    a.AttendanceDate <= endDate &&
                    studentIds.Contains(a.StudentID))
                .Select(a => new { a.StudentID, a.AttendanceDate, a.Status })
                .ToListAsync();

            // Count working days recorded for this class (days where any attendance was marked)
            var workingDays = records
                .Select(r => r.AttendanceDate.Date)
                .Distinct()
                .Count();

            var byStudent = records.GroupBy(r => r.StudentID)
                .ToDictionary(g => g.Key, g => g.ToList());

            var rows = students.Select(s =>
            {
                byStudent.TryGetValue(s.StudentID, out var attended);
                attended ??= new();

                var present = attended.Count(a => a.Status == AttendanceStatus.Present);
                var absent  = attended.Count(a => a.Status == AttendanceStatus.Absent);
                var late    = attended.Count(a => a.Status == AttendanceStatus.Late);
                var leave   = attended.Count(a => a.Status == AttendanceStatus.Leave);
                var total   = present + absent + late + leave;
                var pct     = total > 0 ? Math.Round((double)(present + late) / total * 100, 1) : 0.0;

                return new
                {
                    s.StudentID,
                    s.StudentName,
                    s.RegistrationNo,
                    Present      = present,
                    Absent       = absent,
                    Late         = late,
                    Leave        = leave,
                    TotalMarked  = total,
                    AttendancePct = pct
                };
            }).ToList();

            return Ok(new
            {
                CurrentClassID = currentClassId,
                ClassName      = students.FirstOrDefault()?.ClassName,
                Year           = y,
                Month          = m,
                WorkingDays    = workingDays,
                GeneratedAt    = DateTime.UtcNow,
                Rows           = rows
            });
        }

        // ── GET api/reports/current-structure ─────────────────────────────────
        // Structural snapshot of the active term (or a given termId):
        //   • Teachers — class count, student count, class timings
        //   • Classes  — teacher, students, males, females, slot + timing
        //   • Slots    — timing, students, males, females
        //   • Classes without a teacher — class + student count
        [HttpGet("current-structure")]
        public async Task<IActionResult> GetCurrentStructure([FromQuery] Guid? termId = null)
        {
            var term = termId.HasValue
                ? await dbContext.Term.AsNoTracking().FirstOrDefaultAsync(t => t.TermID == termId.Value)
                : (await dbContext.Term.AsNoTracking().Where(t => t.IsActive).OrderByDescending(t => t.TermStart).FirstOrDefaultAsync()
                   ?? await dbContext.Term.AsNoTracking().OrderByDescending(t => t.TermStart).FirstOrDefaultAsync());

            if (term == null)
                return Ok(new
                {
                    GeneratedAt = DateTime.UtcNow,
                    TermID = (Guid?)null, TermName = (string?)null,
                    Teachers = Array.Empty<object>(), Classes = Array.Empty<object>(),
                    Slots = Array.Empty<object>(), ClassesWithoutTeacher = Array.Empty<object>()
                });

            var classes = await dbContext.CurrentClasses.AsNoTracking()
                .Include(cc => cc.Class)
                .Include(cc => cc.Teacher)
                .Include(cc => cc.Slot)
                .Where(cc => cc.TermID == term.TermID)
                .ToListAsync();

            var classIds = classes.Select(c => c.CurrentClassID).ToList();

            var enrol = await dbContext.ClassStudents.AsNoTracking()
                .Where(cs => classIds.Contains(cs.CurrentClassID) && cs.Status == "Enrolled")
                .Select(cs => new { cs.CurrentClassID, cs.Student.Gender })
                .ToListAsync();

            static bool IsMale(string? g)   => !string.IsNullOrWhiteSpace(g) && g.TrimStart().StartsWith("m", StringComparison.OrdinalIgnoreCase);
            static bool IsFemale(string? g) => !string.IsNullOrWhiteSpace(g) && g.TrimStart().StartsWith("f", StringComparison.OrdinalIgnoreCase);

            var byClass = enrol
                .GroupBy(e => e.CurrentClassID)
                .ToDictionary(g => g.Key, g => new
                {
                    Total   = g.Count(),
                    Males   = g.Count(x => IsMale(x.Gender)),
                    Females = g.Count(x => IsFemale(x.Gender)),
                });

            (int Total, int Males, int Females) Counts(Guid ccId) =>
                byClass.TryGetValue(ccId, out var c) ? (c.Total, c.Males, c.Females) : (0, 0, 0);

            static string? SlotTiming(Slots? s)
            {
                if (s == null) return null;
                if (s.StartTime == s.EndTime) return null;   // unset / placeholder
                return $"{s.StartTime:HH:mm} - {s.EndTime:HH:mm}";
            }

            // ── Class report ──
            var classReport = classes.Select(cc =>
            {
                var (t, m, f) = Counts(cc.CurrentClassID);
                return new
                {
                    cc.CurrentClassID,
                    ClassName   = cc.Class?.ClassName,
                    TeacherName = cc.Teacher?.TeacherName,
                    Students    = t,
                    Males       = m,
                    Females     = f,
                    SlotName    = cc.Slot?.SlotName,
                    SlotTiming  = SlotTiming(cc.Slot),
                };
            }).OrderBy(x => x.ClassName).ToList();

            // ── Teachers ──
            var teachers = classes.Where(cc => cc.TeacherID != null)
                .GroupBy(cc => cc.TeacherID!.Value)
                .Select(g =>
                {
                    var name = g.First().Teacher?.TeacherName;
                    var timings = g.Select(cc =>
                    {
                        var cls   = cc.Class?.ClassName;
                        var slot  = cc.Slot?.SlotName;
                        var tm    = SlotTiming(cc.Slot);
                        var label = cls ?? "Class";
                        if (slot != null || tm != null)
                            label += $" ({string.Join(" ", new[] { slot, tm }.Where(s => !string.IsNullOrWhiteSpace(s)))})";
                        return label;
                    }).ToList();

                    return new
                    {
                        TeacherID    = g.Key,
                        TeacherName  = name,
                        ClassCount   = g.Count(),
                        StudentCount = g.Sum(cc => Counts(cc.CurrentClassID).Total),
                        Timings      = timings,
                    };
                }).OrderBy(x => x.TeacherName).ToList();

            // ── Slots ──
            var slots = classes.Where(cc => cc.Slot != null)
                .GroupBy(cc => cc.SlotID!.Value)
                .Select(g =>
                {
                    var slot = g.First().Slot;
                    return new
                    {
                        SlotID   = g.Key,
                        SlotName = slot?.SlotName,
                        Timing   = SlotTiming(slot),
                        Students = g.Sum(cc => Counts(cc.CurrentClassID).Total),
                        Males    = g.Sum(cc => Counts(cc.CurrentClassID).Males),
                        Females  = g.Sum(cc => Counts(cc.CurrentClassID).Females),
                    };
                }).OrderBy(x => x.SlotName).ToList();

            // ── Classes without a teacher ──
            var classesWithoutTeacher = classes.Where(cc => cc.TeacherID == null)
                .Select(cc =>
                {
                    var (t, m, f) = Counts(cc.CurrentClassID);
                    return new
                    {
                        cc.CurrentClassID,
                        ClassName = cc.Class?.ClassName,
                        Students  = t,
                        Males     = m,
                        Females   = f,
                        SlotName  = cc.Slot?.SlotName,
                    };
                }).OrderBy(x => x.ClassName).ToList();

            return Ok(new
            {
                GeneratedAt = DateTime.UtcNow,
                term.TermID,
                term.TermName,
                Teachers = teachers,
                Classes = classReport,
                Slots = slots,
                ClassesWithoutTeacher = classesWithoutTeacher,
            });
        }

        // ── GET api/reports/fee-dues-summary ──────────────────────────────────
        // Overview of all fee dues across the institute: how many are paid,
        // unpaid, partial, and the total amounts. Useful for dashboard cards.
        [HttpGet("fee-dues-summary")]
        public async Task<IActionResult> GetFeeDuesSummary([FromQuery] Guid? courseId = null)
        {
            var query = dbContext.FeeDues
                .AsNoTracking()
                .Include(d => d.PaymentDetails)
                .Include(d => d.Admission)
                .Where(d => d.Admission.IsActive);

            if (courseId.HasValue)
                query = query.Where(d => d.Admission.CourseID == courseId.Value);

            var dues = await query.ToListAsync();

            var today = DateTime.UtcNow.Date;

            var summary = new
            {
                GeneratedAt    = DateTime.UtcNow,
                TotalDues      = dues.Count,
                TotalBaseAmount = dues.Sum(d => d.BaseAmount),
                TotalLateFee   = dues.Sum(d => d.IsLateFeeWaived ? 0m : d.LateFeeAmount),
                TotalBilled    = dues.Sum(d => d.BaseAmount + (d.IsLateFeeWaived ? 0m : d.LateFeeAmount)),
                TotalCollected = dues.SelectMany(d => d.PaymentDetails).Sum(p => p.PaidAmount),
                ByStatus       = dues
                    .GroupBy(d => d.Status.ToString())
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count  = g.Count(),
                        Amount = g.Sum(d => d.BaseAmount + (d.IsLateFeeWaived ? 0m : d.LateFeeAmount))
                    }),
                ByFeeType      = dues
                    .GroupBy(d => d.FeeType.ToString())
                    .Select(g => new
                    {
                        FeeType  = g.Key,
                        Count    = g.Count(),
                        Billed   = g.Sum(d => d.BaseAmount + (d.IsLateFeeWaived ? 0m : d.LateFeeAmount)),
                        Collected = g.SelectMany(d => d.PaymentDetails).Sum(p => p.PaidAmount)
                    }),
                OverdueCount   = dues.Count(d => d.DueDate < today &&
                                    (d.Status == FeeDueStatus.Unpaid || d.Status == FeeDueStatus.Partial) &&
                                    (d.BaseAmount + (d.IsLateFeeWaived ? 0m : d.LateFeeAmount)) > 0m &&
                                    d.Status != FeeDueStatus.NR)
            };

            return Ok(summary);
        }
    }
}
