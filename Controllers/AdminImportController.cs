using InstituteWebAPI.Data;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminImportController : ControllerBase
    {
        private readonly RozhnInstituteDbContext db;
        private readonly IWebHostEnvironment env;
        private readonly IHttpContextAccessor http;

        public AdminImportController(RozhnInstituteDbContext db, IWebHostEnvironment env, IHttpContextAccessor http)
        {
            this.db = db; this.env = env; this.http = http;
        }

        // ── helpers ───────────────────────────────────────────────────────────
        private static string S(string? v) => (v ?? "").Trim();
        private static decimal Dec(decimal? v) => v ?? 0m;
        private static DateTime? Date(string? v)
        {
            if (string.IsNullOrWhiteSpace(v)) return null;
            if (DateTime.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return d;
            if (DateTime.TryParse(v, out d)) return d;
            return null;
        }
        private static int MonthNum(string? m)
        {
            m = S(m);
            if (int.TryParse(m, out var n) && n >= 1 && n <= 12) return n;
            var formats = new[] { "MMMM", "MMM" };
            foreach (var f in formats)
                if (DateTime.TryParseExact(m, f, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return d.Month;
            return 0;
        }
        // Due date can be a day-of-month number (e.g. "5") or a full date — return day 1-31.
        private static int? DayOfMonth(string? v)
        {
            v = S(v);
            if (v.Length == 0) return null;
            if (int.TryParse(v, out var n) && n >= 1 && n <= 31) return n;
            var d = Date(v);
            return d?.Day;
        }

        public class ImportResult
        {
            public int Created { get; set; }
            public int Skipped { get; set; }
            public int Updated { get; set; }
            public List<string> Errors { get; set; } = new();
        }

        // ════════ STUDENTS (+ admission) ════════
        public class StudentRow
        {
            public string? RegNo { get; set; }
            public string? Name { get; set; }
            public string? FatherName { get; set; }
            public string? Gender { get; set; }
            public string? Occupation { get; set; }
            public string? Qualification { get; set; }
            public string? Institute { get; set; }
            public string? Cnic { get; set; }
            public string? Contact { get; set; }
            public string? Region { get; set; }
            public string? Address { get; set; }
            public string? Language { get; set; }       // → class name
            public string? AdmissionDate { get; set; }
            public decimal? CourseAdmission { get; set; } // admission fee
            public decimal? Tuition { get; set; }         // monthly fee
            public string? Status { get; set; }
        }

        [HttpPost("students")]
        public async Task<IActionResult> ImportStudents([FromBody] List<StudentRow> rows)
        {
            var res = new ImportResult();
            if (rows == null || rows.Count == 0) return Ok(res);

            // Students only — admissions/classes are created separately from the 2026 sheet.
            var villages = await db.Village.ToListAsync();
            var maxSerial = await db.Students.MaxAsync(s => (int?)s.Serial) ?? 0;

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                var reg = S(r.RegNo);
                if (reg.Length == 0) { res.Errors.Add($"Row {i + 2}: missing RegNo"); continue; }
                if (await db.Students.AnyAsync(s => s.RegistrationNo == reg)) { res.Skipped++; continue; }

                try
                {
                    // Village from Region (create if new)
                    var regionName = S(r.Region);
                    Village? village = null;
                    if (regionName.Length > 0)
                        village = villages.FirstOrDefault(v => v.VillageName.Equals(regionName, StringComparison.OrdinalIgnoreCase));
                    if (village == null)
                    {
                        var name = regionName.Length > 0 ? regionName : "Unspecified";
                        village = villages.FirstOrDefault(v => v.VillageName.Equals(name, StringComparison.OrdinalIgnoreCase));
                        if (village == null)
                        {
                            village = new Village { VillageID = Guid.NewGuid(), VillageName = name };
                            db.Village.Add(village); villages.Add(village);
                        }
                    }

                    var student = new Students
                    {
                        StudentID = Guid.NewGuid(),
                        Serial = ++maxSerial,
                        RegistrationNo = reg,
                        RegDate = Date(r.AdmissionDate) ?? DateTime.Now,
                        StudentName = S(r.Name),
                        FatherName = S(r.FatherName),
                        Gender = S(r.Gender).Length > 0 ? S(r.Gender) : "—",
                        DateOfBirth = new DateTime(1900, 1, 1),  // not in source
                        VillageID = village.VillageID,
                        Address = S(r.Address),
                        City = "",
                        FatherContact = S(r.Contact),
                        FatherOccupation = S(r.Occupation),
                        Qualification = S(r.Qualification),
                        Institute = S(r.Institute),
                        FatherCnic = S(r.Cnic),
                        Picture = "",
                        CreatedAt = DateTime.Now,
                        ModifiedAt = DateTime.Now,
                        IsEnrolled = !S(r.Status).Equals("Left", StringComparison.OrdinalIgnoreCase),
                    };
                    db.Students.Add(student);

                    await db.SaveChangesAsync();
                    res.Created++;
                }
                catch (Exception ex)
                {
                    res.Errors.Add($"Row {i + 2} ({reg}): {ex.Message}");
                }
            }
            return Ok(res);
        }

        // ════════ RESET (wipe students + admissions + dependents) ════════
        // Destructive: deletes ALL students, admissions and every record that
        // depends on them. Structural data (classes, slots, teachers, terms) is kept.
        [HttpPost("reset")]
        public async Task<IActionResult> ResetAll([FromQuery] string? confirm)
        {
            if (confirm != "DELETE")
                return BadRequest(new { message = "Confirmation required. Pass confirm=DELETE." });

            using var tx = await db.Database.BeginTransactionAsync();

            // Children → parents (FK-safe order).
            await db.PaymentDetails.ExecuteDeleteAsync();
            await db.Payments.ExecuteDeleteAsync();
            await db.FeeDues.ExecuteDeleteAsync();
            await db.MarkEditRequests.ExecuteDeleteAsync();
            await db.StudentMarks.ExecuteDeleteAsync();
            await db.StudentMonthlyResults.ExecuteDeleteAsync();
            await db.TerminalResults.ExecuteDeleteAsync();
            await db.StudentAttendances.ExecuteDeleteAsync();
            await db.AdmitCards.ExecuteDeleteAsync();
            await db.CardRequests.ExecuteDeleteAsync();
            await db.StudentLeaveRequests.ExecuteDeleteAsync();
            await db.Scholarships.ExecuteDeleteAsync();
            await db.StudentFeeHistories.ExecuteDeleteAsync();
            await db.Guardians.ExecuteDeleteAsync();
            await db.ClassStudents.ExecuteDeleteAsync();
            await db.Admissions.ExecuteDeleteAsync();
            await db.Students.ExecuteDeleteAsync();

            await tx.CommitAsync();
            return Ok(new { message = "All students, admissions and related records have been deleted." });
        }

        // ════════ DIAGNOSTICS ════════
        // Quick snapshot to debug "no enrolled students" issues.
        [HttpGet("diagnostics")]
        public async Task<IActionResult> Diagnostics()
        {
            var term = await db.Term.Where(t => t.IsActive).OrderByDescending(t => t.TermStart).FirstOrDefaultAsync();
            var termId = term?.TermID;

            var statusBreakdown = await db.ClassStudents
                .GroupBy(cs => cs.Status)
                .Select(g => new { Status = g.Key ?? "(null)", Count = g.Count() })
                .ToListAsync();

            var ccInActiveTerm = termId.HasValue
                ? await db.CurrentClasses.CountAsync(cc => cc.TermID == termId.Value)
                : 0;

            var enrolInActiveTerm = termId.HasValue
                ? await db.ClassStudents.CountAsync(cs => cs.CurrentClass.TermID == termId.Value)
                : 0;

            // Top current classes in the active term by enrolment count
            var topClasses = termId.HasValue
                ? await db.CurrentClasses
                    .Where(cc => cc.TermID == termId.Value)
                    .Select(cc => new
                    {
                        cc.CurrentClassID,
                        ClassName = cc.Class != null ? cc.Class.ClassName : "(none)",
                        Teacher   = cc.Teacher != null ? cc.Teacher.TeacherName : "(unassigned)",
                        Enrolled  = cc.ClassStudents.Count(x => x.Status == "Enrolled"),
                        Total     = cc.ClassStudents.Count(),
                    })
                    .OrderByDescending(x => x.Total)
                    .Take(15)
                    .ToListAsync()
                : null;

            return Ok(new
            {
                ActiveTerm = term == null ? null : new { term.TermID, term.TermName, term.IsActive, term.TermStart },
                TotalStudents       = await db.Students.CountAsync(),
                TotalAdmissions     = await db.Admissions.CountAsync(),
                TotalTests          = await db.Tests.CountAsync(),
                TotalStudentMarks   = await db.StudentMarks.CountAsync(),
                TotalMonthlyResults = await db.StudentMonthlyResults.CountAsync(),
                TotalTerminalResults= await db.TerminalResults.CountAsync(),
                ActiveAdmissions    = await db.Admissions.CountAsync(a => a.IsActive),
                TotalClassStudents  = await db.ClassStudents.CountAsync(),
                CurrentClassesInActiveTerm = ccInActiveTerm,
                EnrolmentsInActiveTerm     = enrolInActiveTerm,
                StatusBreakdown = statusBreakdown,
                TopClasses = topClasses
            });
        }

        // ════════ FIX enrolment status (Active → Enrolled) ════════
        // Older imports wrote ClassStudents.Status = "Active"; the app expects
        // "Enrolled". This normalises any leftover rows so attendance/marks show them.
        [HttpPost("fix-enrolment-status")]
        public async Task<IActionResult> FixEnrolmentStatus()
        {
            var updated = await db.ClassStudents
                .Where(cs => cs.Status == "Active")
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "Enrolled"));
            return Ok(new { updated, message = $"{updated} enrolment(s) set to Enrolled." });
        }

        // ════════ REBUILD MARKS from monthly results ════════
        // Generates a month Test per class+month and a StudentMarks row per student
        // from the existing StudentMonthlyResults, so the Marks page shows numbers.
        [HttpPost("rebuild-marks")]
        public async Task<IActionResult> RebuildMarks()
        {
            var monthly = await db.StudentMonthlyResults.AsNoTracking().ToListAsync();
            var tests   = await db.Tests.ToListAsync();
            var existing = await db.StudentMarks.AsNoTracking()
                .Select(m => new { m.TestID, m.StudentID })
                .ToListAsync();
            var markSet = new HashSet<(Guid, Guid)>(existing.Select(x => (x.TestID, x.StudentID)));

            int testsCreated = 0, marksCreated = 0;
            foreach (var mr in monthly)
            {
                var totalMarks = mr.TotalMarks > 0 ? mr.TotalMarks : 100f;

                var test = tests.FirstOrDefault(t => t.CurrentClassID == mr.CurrentClassID && t.TermMonthID == mr.TermMonthID);
                if (test == null)
                {
                    test = new Tests
                    {
                        TestID = Guid.NewGuid(),
                        CurrentClassID = mr.CurrentClassID,
                        TermMonthID = mr.TermMonthID,
                        TestType = "Monthly",
                        TotalMarks = totalMarks,
                    };
                    db.Tests.Add(test); tests.Add(test); testsCreated++;
                }

                if (!markSet.Contains((test.TestID, mr.StudentID)))
                {
                    db.StudentMarks.Add(new StudentMarks
                    {
                        StudentMarkID = Guid.NewGuid(),
                        TestID = test.TestID,
                        StudentID = mr.StudentID,
                        TermID = mr.TermID,
                        ObtainedMarks = mr.ObtainedMarks,
                        TotalMarks = totalMarks,
                        Percentage = mr.Percentage,
                    });
                    markSet.Add((test.TestID, mr.StudentID));
                    marksCreated++;
                }
            }

            // Repair terminal percentages (stored as Excel fractions, 0.8 → 80) and
            // recompute Pass/Fail against the configured terminal passing %.
            var passingLookup = await db.TerminalPassingMarks
                .AsNoTracking()
                .ToDictionaryAsync(p => (p.TermID, p.CurrentClassID), p => p.PassingMarks);

            var terminals = await db.TerminalResults.ToListAsync();
            int termFixed = 0, resultFixed = 0;
            foreach (var t in terminals)
            {
                if (t.TotalMarksConsidered > 0)
                {
                    var newPct = t.TotalObtained / t.TotalMarksConsidered * 100f;
                    if (Math.Abs(newPct - t.Percentage) > 0.05f)
                    {
                        t.Percentage = newPct;
                        termFixed++;
                    }
                }

                // Promoted is a manual decision — leave it alone.
                if (!string.Equals(t.Result, "Promoted", StringComparison.OrdinalIgnoreCase))
                {
                    var passing = passingLookup.TryGetValue((t.TermID, t.CurrentClassID), out var pm) ? pm : 45f;
                    if (passing <= 1.5f) passing *= 100f;   // stored as a fraction → percent
                    var shouldFail = t.Percentage < passing;
                    if (shouldFail && !string.Equals(t.Result, "Fail", StringComparison.OrdinalIgnoreCase))
                    {
                        t.Result = "Fail";
                        resultFixed++;
                    }
                    else if (!shouldFail && string.Equals(t.Result, "Fail", StringComparison.OrdinalIgnoreCase))
                    {
                        t.Result = "Pass";
                        resultFixed++;
                    }
                }
            }

            await db.SaveChangesAsync();
            return Ok(new { testsCreated, marksCreated, terminalPercentFixed = termFixed, terminalResultFixed = resultFixed,
                message = $"{marksCreated} marks created; {termFixed} percentages fixed; {resultFixed} pass/fail corrected." });
        }

        // ════════ ADMISSIONS from the 2026 sheet ════════
        // Creates admissions (class, slot, current-class enrolment, due date, fee)
        // for students that already exist (matched by Reg No.).
        public class AdmissionRow
        {
            public string? RegNo { get; set; }
            public string? AdmissionDate { get; set; }
            public string? Slot { get; set; }
            public string? Teacher { get; set; }
            public string? ClassName { get; set; }
            public string? DueDate { get; set; }   // day-of-month or a date
            public decimal? Fee { get; set; }      // monthly fee
            public string? Status { get; set; }
        }

        [HttpPost("admissions-2026")]
        public async Task<IActionResult> ImportAdmissions2026([FromBody] List<AdmissionRow> rows)
        {
            var res = new ImportResult();
            if (rows == null || rows.Count == 0) return Ok(res);

            var term = await db.Term.Where(t => t.IsActive).OrderByDescending(t => t.TermStart).FirstOrDefaultAsync();
            if (term == null) { res.Errors.Add("No active term found. Set an active term first."); return Ok(res); }

            var course = await db.Courses.FirstOrDefaultAsync(c => c.CourseName == "Language");
            if (course == null)
            {
                course = new Courses { CourseID = Guid.NewGuid(), CourseName = "Language", CourseDescription = "Imported", CourseStatus = true };
                db.Courses.Add(course); await db.SaveChangesAsync();
            }

            var classes        = await db.Classes.Where(c => c.CourseID == course.CourseID).ToListAsync();
            var slots          = await db.Slots.Where(s => s.CourseID == course.CourseID).ToListAsync();
            var teachers       = await db.Teachers.ToListAsync();
            var currentClasses = await db.CurrentClasses.Where(cc => cc.TermID == term.TermID).ToListAsync();

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                var reg = S(r.RegNo);
                if (reg.Length == 0) { res.Errors.Add($"Row {i + 2}: missing Reg No."); continue; }

                try
                {
                    var student = await db.Students.FirstOrDefaultAsync(s => s.RegistrationNo == reg);
                    if (student == null) { res.Errors.Add($"Row {i + 2}: student {reg} not found — import the Student List first."); continue; }

                    // Class
                    var className = S(r.ClassName);
                    Classes? cls = null;
                    if (className.Length > 0)
                    {
                        cls = classes.FirstOrDefault(c => c.ClassName.Equals(className, StringComparison.OrdinalIgnoreCase));
                        if (cls == null)
                        {
                            cls = new Classes { ClassID = Guid.NewGuid(), ClassName = className, CourseID = course.CourseID, Rank = 0 };
                            db.Classes.Add(cls); classes.Add(cls);
                        }
                    }

                    // Slot
                    var slotName = S(r.Slot);
                    Slots? slot = null;
                    if (slotName.Length > 0)
                    {
                        slot = slots.FirstOrDefault(s => s.SlotName.Equals(slotName, StringComparison.OrdinalIgnoreCase));
                        if (slot == null)
                        {
                            slot = new Slots
                            {
                                SlotID = Guid.NewGuid(),
                                SlotName = slotName,
                                CourseID = course.CourseID,
                                TermID = term.TermID,
                                StartTime = DateTime.Today,
                                EndTime = DateTime.Today,
                            };
                            db.Slots.Add(slot); slots.Add(slot);
                        }
                    }

                    // Teacher (match by name only; leave unassigned if not found)
                    var teacherName = S(r.Teacher);
                    Teachers? teacher = teacherName.Length > 0
                        ? teachers.FirstOrDefault(t => t.TeacherName.Equals(teacherName, StringComparison.OrdinalIgnoreCase))
                        : null;

                    // Current class for the active term (find or create)
                    CurrentClass? cc = null;
                    if (cls != null)
                    {
                        cc = currentClasses.FirstOrDefault(x => x.ClassID == cls.ClassID
                                && x.SlotID == slot?.SlotID
                                && x.TeacherID == teacher?.TeacherID
                                && x.TermID == term.TermID);
                        if (cc == null)
                        {
                            cc = new CurrentClass
                            {
                                CurrentClassID = Guid.NewGuid(),
                                ClassID = cls.ClassID,
                                SlotID = slot?.SlotID,
                                TeacherID = teacher?.TeacherID,
                                TermID = term.TermID,
                                IsActive = true,
                                CreatedOn = DateTime.Now,
                            };
                            db.CurrentClasses.Add(cc); currentClasses.Add(cc);
                        }
                    }

                    // Admission (update active one if present, else create)
                    var dueDay  = DayOfMonth(r.DueDate);
                    var regDate = Date(r.AdmissionDate) ?? DateTime.Now;
                    var fee     = Dec(r.Fee);

                    var admission = await db.Admissions
                        .Where(a => a.StudentID == student.StudentID && a.IsActive)
                        .OrderByDescending(a => a.RegistrationDate)
                        .FirstOrDefaultAsync();

                    if (admission == null)
                    {
                        db.Admissions.Add(new Admissions
                        {
                            AdmissionID = Guid.NewGuid(),
                            StudentID = student.StudentID,
                            CourseID = course.CourseID,
                            AdmittedClassID = cls?.ClassID,
                            RegistrationDate = regDate,
                            MonthlyFee = fee,
                            AdmissionFee = 0m,
                            DueDate = dueDay,
                            Status = "Active",
                            IsActive = true,
                            IsFree = false,
                            CreatedAt = DateTime.Now,
                            ModifiedAt = DateTime.Now,
                        });
                        res.Created++;
                    }
                    else
                    {
                        admission.CourseID = course.CourseID;
                        admission.AdmittedClassID = cls?.ClassID ?? admission.AdmittedClassID;
                        admission.RegistrationDate = regDate;
                        admission.MonthlyFee = fee;
                        admission.DueDate = dueDay;
                        admission.ModifiedAt = DateTime.Now;
                        res.Updated++;
                    }

                    // Enrolment in the active term (one per student per term)
                    if (cc != null)
                    {
                        var existingEnroll = await db.ClassStudents
                            .Include(e => e.CurrentClass)
                            .FirstOrDefaultAsync(e => e.StudentID == student.StudentID && e.CurrentClass.TermID == term.TermID);
                        if (existingEnroll == null)
                        {
                            db.ClassStudents.Add(new ClassStudents
                            {
                                ClassStudentID = Guid.NewGuid(),
                                CurrentClassID = cc.CurrentClassID,
                                StudentID = student.StudentID,
                                Status = "Enrolled",
                            });
                        }
                        else
                        {
                            existingEnroll.CurrentClassID = cc.CurrentClassID;
                            existingEnroll.Status = "Enrolled";
                        }
                    }

                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    res.Errors.Add($"Row {i + 2} ({reg}): {ex.Message}");
                }
            }

            // Normalise any leftover enrolment status so attendance/marks show students.
            await db.ClassStudents
                .Where(cs => cs.Status == "Active")
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "Enrolled"));

            return Ok(res);
        }

        // ════════ MARKS / RESULTS (from the Result Sheet) ════════
        // Full import: terms, classes, enrolments, monthly tests (1 & 2 out of 100,
        // passing 45), the 3rd-month component tests (term-scoped test types that
        // split 100), per-student marks (NI/NC skipped), monthly results and the
        // calculated terminal result.
        public class MarkRow
        {
            public string? Reg { get; set; }
            public string? TermNo { get; set; }        // = term name
            public string? Date { get; set; }
            public string? StudentName { get; set; }    // used to auto-create a missing student
            public string? FatherName { get; set; }
            public string? ClassName { get; set; }
            public decimal? Month1 { get; set; }
            public decimal? Month2 { get; set; }
            // 3rd-month components
            public decimal? Written { get; set; }
            public decimal? Wordlist { get; set; }
            public decimal? Viva { get; set; }
            public decimal? Presentation { get; set; }
            public decimal? Conversation { get; set; }
            public decimal? SpontCommunication { get; set; }
            public decimal? GroupTask { get; set; }
            public decimal? Debate { get; set; }
            public decimal? Performance { get; set; }
            public decimal? BookReview { get; set; }
            public decimal? CompAttendance { get; set; }
            public decimal? Assignment { get; set; }
            public decimal? Facilitators { get; set; }
            public decimal? Total { get; set; }
            public decimal? Obtained { get; set; }
            public decimal? Percentage { get; set; }
            public decimal? PassingPercent { get; set; }
            public string? Result { get; set; }
            public string? ModifiedResult { get; set; }
            public string? Grade { get; set; }
        }

        private static readonly (string Name, Func<MarkRow, decimal?> Get)[] Month3Components =
            new (string, Func<MarkRow, decimal?>)[]
        {
            ("Written",                  r => r.Written),
            ("Wordlist",                 r => r.Wordlist),
            ("Viva",                     r => r.Viva),
            ("Presentation",             r => r.Presentation),
            ("Conversation",             r => r.Conversation),
            ("Spont. Communication",     r => r.SpontCommunication),
            ("Group Task/Surprise Test", r => r.GroupTask),
            ("Debate",                   r => r.Debate),
            ("Performance",              r => r.Performance),
            ("Book Review",              r => r.BookReview),
            ("Attendance",               r => r.CompAttendance),
            ("Assignment",               r => r.Assignment),
            ("Facilitators",             r => r.Facilitators),
        };

        [HttpPost("marks")]
        public async Task<IActionResult> ImportMarks([FromBody] List<MarkRow> rows)
        {
            var res = new ImportResult();
            if (rows == null || rows.Count == 0) return Ok(res);

            var course = await db.Courses.FirstOrDefaultAsync(c => c.CourseName == "Language");
            if (course == null)
            {
                course = new Courses { CourseID = Guid.NewGuid(), CourseName = "Language", CourseDescription = "Imported", CourseStatus = true };
                db.Courses.Add(course); await db.SaveChangesAsync();
            }

            const float PassMark = 45f;

            // Clear previous marks data for a clean re-import.
            await db.StudentMarks.ExecuteDeleteAsync();
            await db.Tests.ExecuteDeleteAsync();
            await db.StudentMonthlyResults.ExecuteDeleteAsync();
            await db.TerminalResults.ExecuteDeleteAsync();
            await db.TermMonthPassingMarks.ExecuteDeleteAsync();
            await db.TerminalPassingMarks.ExecuteDeleteAsync();

            var terms          = await db.Term.ToListAsync();
            var classes        = await db.Classes.Where(c => c.CourseID == course.CourseID).ToListAsync();
            var currentClasses = await db.CurrentClasses.ToListAsync();
            var termMonths     = await db.TermMonths.ToListAsync();
            var testTypes      = await db.TestTypes.ToListAsync();
            var villages       = await db.Village.ToListAsync();
            var maxSerial      = await db.Students.MaxAsync(s => (int?)s.Serial) ?? 0;
            var tests          = new List<Tests>();                       // freshly cleared
            var passingMonthDone    = new HashSet<(Guid, Guid)>();         // (cc, termMonth)
            var passingTerminalDone = new HashSet<(Guid, Guid)>();         // (term, cc)
            var seenStudentClass    = new HashSet<(Guid, Guid, Guid)>();   // (term, cc, student) — dedupe duplicate sheet rows

            Village DefaultVillage()
            {
                var v = villages.FirstOrDefault(x => x.VillageName == "Unspecified");
                if (v == null) { v = new Village { VillageID = Guid.NewGuid(), VillageName = "Unspecified" }; db.Village.Add(v); villages.Add(v); }
                return v;
            }

            TermMonths GetMonth(int n)
            {
                var tm = termMonths.FirstOrDefault(x => x.TermMonth == n);
                if (tm == null) { tm = new TermMonths { TermMonthID = Guid.NewGuid(), TermMonth = n }; db.TermMonths.Add(tm); termMonths.Add(tm); }
                return tm;
            }
            void EnsureTestType(string name, Guid termId)
            {
                if (!testTypes.Any(t => t.TermID == termId && t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    var tt = new TestType { TestTypeID = Guid.NewGuid(), Name = name, TermID = termId, CreatedAt = DateTime.Now, ModifiedAt = DateTime.Now };
                    db.TestTypes.Add(tt); testTypes.Add(tt);
                }
            }
            Tests GetOrCreateTest(Guid ccId, Guid tmId, string testType, float total)
            {
                var t = tests.FirstOrDefault(x => x.CurrentClassID == ccId && x.TermMonthID == tmId && x.TestType == testType);
                if (t == null)
                {
                    t = new Tests { TestID = Guid.NewGuid(), CurrentClassID = ccId, TermMonthID = tmId, TestType = testType, TotalMarks = total };
                    db.Tests.Add(t); tests.Add(t);
                }
                return t;
            }
            void EnsureMonthPassing(Guid termId, Guid ccId, Guid tmId)
            {
                if (passingMonthDone.Add((ccId, tmId)))
                    db.TermMonthPassingMarks.Add(new TermMonthPassingMark { TermMonthPassingMarkID = Guid.NewGuid(), TermID = termId, CurrentClassID = ccId, TermMonthID = tmId, PassingMarks = PassMark });
            }
            void EnsureTerminalPassing(Guid termId, Guid ccId, float passing)
            {
                if (passingTerminalDone.Add((termId, ccId)))
                    db.TerminalPassingMarks.Add(new TerminalPassingMark { TerminalPassingMarkID = Guid.NewGuid(), TermID = termId, CurrentClassID = ccId, PassingMarks = passing, CreatedAt = DateTime.UtcNow });
            }

            // Pass A: per (term|class), which 3rd-month components carry data → they split 100.
            var compByGroup = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var r0 in rows)
            {
                var tn = S(r0.TermNo); var cn = S(r0.ClassName);
                if (tn.Length == 0 || cn.Length == 0) continue;
                var key = $"{tn}|{cn}";
                if (!compByGroup.TryGetValue(key, out var set)) { set = new HashSet<string>(StringComparer.OrdinalIgnoreCase); compByGroup[key] = set; }
                foreach (var c in Month3Components) if (c.Get(r0).HasValue) set.Add(c.Name);
            }

            var m1Month = GetMonth(1);
            var m2Month = GetMonth(2);
            var m3Month = GetMonth(3);

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                var reg = S(r.Reg);
                if (reg.Length == 0) { res.Errors.Add($"Row {i + 2}: missing Reg"); continue; }

                var termName = S(r.TermNo);
                if (termName.Length == 0) { res.Errors.Add($"Row {i + 2} ({reg}): missing TermNo."); continue; }

                try
                {
                    var student = await db.Students.FirstOrDefaultAsync(s => s.RegistrationNo == reg);
                    if (student == null)
                    {
                        // Auto-create a minimal student from the sheet (historical students
                        // not present in the current Student List).
                        var village = DefaultVillage();
                        student = new Students
                        {
                            StudentID        = Guid.NewGuid(),
                            Serial           = ++maxSerial,
                            RegistrationNo   = reg,
                            RegDate          = Date(r.Date) ?? DateTime.Now,
                            StudentName      = S(r.StudentName).Length > 0 ? S(r.StudentName) : reg,
                            FatherName       = S(r.FatherName),
                            Gender           = "—",
                            DateOfBirth      = new DateTime(1900, 1, 1),
                            VillageID        = village.VillageID,
                            Address          = "",
                            City             = "",
                            FatherContact    = "",
                            FatherOccupation = "",
                            Qualification    = "",
                            Institute        = "",
                            FatherCnic       = "",
                            Picture          = "",
                            CreatedAt        = DateTime.Now,
                            ModifiedAt       = DateTime.Now,
                            IsEnrolled       = true,
                        };
                        db.Students.Add(student);
                        await db.SaveChangesAsync();
                    }

                    // Term (find by name, create if missing)
                    var term = terms.FirstOrDefault(t => t.TermName.Equals(termName, StringComparison.OrdinalIgnoreCase));
                    if (term == null)
                    {
                        var start = Date(r.Date) ?? DateTime.Now;
                        term = new Term
                        {
                            TermID = Guid.NewGuid(),
                            TermName = termName,
                            TermStart = start,
                            TermEnd = start.AddMonths(3),
                            TermDuration = "3 months",
                            IsActive = false,
                        };
                        db.Term.Add(term); terms.Add(term);
                    }

                    // Class (create in this course; "New Class"/"Teacher" columns ignored)
                    var className = S(r.ClassName);
                    Classes? cls = null;
                    if (className.Length > 0)
                    {
                        cls = classes.FirstOrDefault(c => c.ClassName.Equals(className, StringComparison.OrdinalIgnoreCase));
                        if (cls == null)
                        {
                            cls = new Classes { ClassID = Guid.NewGuid(), ClassName = className, CourseID = course.CourseID, Rank = 0 };
                            db.Classes.Add(cls); classes.Add(cls);
                        }
                    }
                    if (cls == null) { res.Errors.Add($"Row {i + 2} ({reg}): missing Class"); continue; }

                    // Current class for this class + term (find or create; no teacher/slot)
                    var cc = currentClasses.FirstOrDefault(x => x.ClassID == cls.ClassID && x.TermID == term.TermID);
                    if (cc == null)
                    {
                        cc = new CurrentClass
                        {
                            CurrentClassID = Guid.NewGuid(),
                            ClassID = cls.ClassID,
                            TermID = term.TermID,
                            IsActive = false,
                            CreatedOn = DateTime.Now,
                        };
                        db.CurrentClasses.Add(cc); currentClasses.Add(cc);
                    }

                    // Dedupe: the sheet can list the same student twice for the same
                    // term + class. The first row owns the result; later duplicates are
                    // skipped so they don't violate the unique index on
                    // StudentMonthlyResults (TermID, CurrentClassID, TermMonthID, StudentID).
                    if (!seenStudentClass.Add((term.TermID, cc.CurrentClassID, student.StudentID)))
                    {
                        res.Skipped++;
                        continue;
                    }

                    // ── Months included (from the Total column) ──────────────────
                    var totalAll = (float)Dec(r.Total);
                    int monthsCount = totalAll >= 300 ? 3 : totalAll >= 200 ? 2 : totalAll >= 100 ? 1 : 3;
                    var include1 = monthsCount >= 3;
                    var include2 = monthsCount >= 2;

                    // 3rd month is out of 100: each non-Written component is worth 10,
                    // the Written component takes the remaining marks.
                    var compSet = compByGroup.TryGetValue($"{termName}|{className}", out var cs) ? cs : new HashSet<string>();
                    var nonWrittenCount = compSet.Count(x => !x.Equals("Written", StringComparison.OrdinalIgnoreCase));
                    var writtenTotal = Math.Max(0f, 100f - 10f * nonWrittenCount);

                    // Month 1 (out of 100, passing 45) — NI/NC arrives null → skipped.
                    float m1 = 0f;
                    if (include1)
                    {
                        var t1 = GetOrCreateTest(cc.CurrentClassID, m1Month.TermMonthID, "Month 1", 100f);
                        EnsureMonthPassing(term.TermID, cc.CurrentClassID, m1Month.TermMonthID);
                        if (r.Month1.HasValue)
                        {
                            m1 = (float)r.Month1.Value;
                            db.StudentMarks.Add(new StudentMarks { StudentMarkID = Guid.NewGuid(), TestID = t1.TestID, StudentID = student.StudentID, TermID = term.TermID, ObtainedMarks = m1, TotalMarks = 100f, Percentage = m1 });
                        }
                    }

                    // Month 2
                    float m2 = 0f;
                    if (include2)
                    {
                        var t2 = GetOrCreateTest(cc.CurrentClassID, m2Month.TermMonthID, "Month 2", 100f);
                        EnsureMonthPassing(term.TermID, cc.CurrentClassID, m2Month.TermMonthID);
                        if (r.Month2.HasValue)
                        {
                            m2 = (float)r.Month2.Value;
                            db.StudentMarks.Add(new StudentMarks { StudentMarkID = Guid.NewGuid(), TestID = t2.TestID, StudentID = student.StudentID, TermID = term.TermID, ObtainedMarks = m2, TotalMarks = 100f, Percentage = m2 });
                        }
                    }

                    // Month 3 — one test per component (term-scoped test type) splitting 100.
                    float m3Obtained = 0f;
                    EnsureMonthPassing(term.TermID, cc.CurrentClassID, m3Month.TermMonthID);
                    foreach (var comp in Month3Components)
                    {
                        if (!compSet.Contains(comp.Name)) continue;
                        EnsureTestType(comp.Name, term.TermID);
                        var compTotal = comp.Name.Equals("Written", StringComparison.OrdinalIgnoreCase) ? writtenTotal : 10f;
                        var ct = GetOrCreateTest(cc.CurrentClassID, m3Month.TermMonthID, comp.Name, compTotal);
                        var val = comp.Get(r);   // NI/NC → null → skip this student's mark
                        if (val.HasValue)
                        {
                            var v = (float)val.Value;
                            m3Obtained += v;
                            db.StudentMarks.Add(new StudentMarks { StudentMarkID = Guid.NewGuid(), TestID = ct.TestID, StudentID = student.StudentID, TermID = term.TermID, ObtainedMarks = v, TotalMarks = compTotal, Percentage = compTotal > 0 ? v / compTotal * 100f : 0f });
                        }
                    }
                    m3Obtained = Math.Min(100f, m3Obtained);

                    // Monthly results (used by the result card)
                    void AddMonthly(Guid tmId, float obt, float tot, string? status = null)
                    {
                        db.StudentMonthlyResults.Add(new StudentMonthlyResult { StudentMonthlyResultID = Guid.NewGuid(), StudentID = student.StudentID, TermID = term.TermID, CurrentClassID = cc.CurrentClassID, TermMonthID = tmId, ObtainedMarks = obt, TotalMarks = tot, Percentage = tot > 0 ? obt / tot * 100f : 0f, Status = status, CreatedOn = DateTime.UtcNow });
                    }
                    // No mark / NI / NC for month 1 or 2 → mark the month "Not Conducted".
                    if (include1) AddMonthly(m1Month.TermMonthID, m1, 100f, r.Month1.HasValue ? null : "Not Conducted");
                    if (include2) AddMonthly(m2Month.TermMonthID, m2, 100f, r.Month2.HasValue ? null : "Not Conducted");
                    AddMonthly(m3Month.TermMonthID, m3Obtained, 100f);

                    // Enrol the student into this class for the term so the class
                    // shows its students (one enrolment per student per term).
                    var existingEnroll = await db.ClassStudents
                        .Include(e => e.CurrentClass)
                        .FirstOrDefaultAsync(e => e.StudentID == student.StudentID && e.CurrentClass.TermID == term.TermID);
                    if (existingEnroll == null)
                    {
                        db.ClassStudents.Add(new ClassStudents
                        {
                            ClassStudentID = Guid.NewGuid(),
                            CurrentClassID = cc.CurrentClassID,
                            StudentID = student.StudentID,
                            Status = "Enrolled",
                        });
                    }
                    else
                    {
                        existingEnroll.CurrentClassID = cc.CurrentClassID;
                        existingEnroll.Status = "Enrolled";
                    }

                    // Terminal result — calculated from the imported marks.
                    // Passing % comes from the sheet (may be a fraction 0.45 or a number 45).
                    var passingRaw      = (float)Dec(r.PassingPercent);
                    var terminalPassing = passingRaw <= 0f ? 45f
                                        : (passingRaw <= 1.5f ? passingRaw * 100f : passingRaw);
                    EnsureTerminalPassing(term.TermID, cc.CurrentClassID, terminalPassing);
                    var consideredTotal = monthsCount * 100f;
                    var obtainedTotal   = m1 + m2 + m3Obtained;
                    var pct             = consideredTotal > 0 ? obtainedTotal / consideredTotal * 100f : 0f;
                    var modified        = S(r.ModifiedResult);
                    var result          = modified.ToLowerInvariant().Contains("promot") ? "Promoted"
                                        : (pct >= terminalPassing ? "Pass" : "Fail");
                    var grade           = S(r.Grade);

                    db.TerminalResults.Add(new TerminalResult
                    {
                        TerminalResultID     = Guid.NewGuid(),
                        StudentID            = student.StudentID,
                        TermID               = term.TermID,
                        CurrentClassID       = cc.CurrentClassID,
                        Month3TestID         = Guid.Empty,
                        Month1TestID         = null,
                        IncludeMonth1        = include1,
                        Month1ObtainedMarks  = m1,
                        Month1TotalMarks     = include1 ? 100f : 0f,
                        Month2TestID         = null,
                        IncludeMonth2        = include2,
                        Month2ObtainedMarks  = m2,
                        Month2TotalMarks     = include2 ? 100f : 0f,
                        Month3ObtainedMarks  = m3Obtained,
                        Month3TotalMarks     = 100f,
                        TotalMarksConsidered = consideredTotal,
                        TotalObtained        = obtainedTotal,
                        Percentage           = pct,
                        Grade                = grade.Length > 0 ? grade : null,
                        Result               = result,
                        IsResultManual       = true,
                        CreatedOn            = DateTime.UtcNow,
                        UpdatedOn            = DateTime.UtcNow,
                    });

                    await db.SaveChangesAsync();
                    res.Created++;
                }
                catch (Exception ex)
                {
                    res.Errors.Add($"Row {i + 2} ({reg}): {ex.Message}");
                }
            }
            return Ok(res);
        }

        // ════════ FEE HISTORY (from Payment sheet) ════════
        public class FeeRow
        {
            public string? StudentId { get; set; }   // = RegNo
            public string? Month { get; set; }
            public int? Year { get; set; }
            public decimal? AdmissionAmount { get; set; }
            public decimal? CardAmount { get; set; }
            public decimal? LateFee { get; set; }
            public decimal? MonthlyFee { get; set; }
            public string? PaidDate { get; set; }
            public decimal? Total { get; set; }
            public string? ReceiptNo { get; set; }
        }

        [HttpPost("fee-history")]
        public async Task<IActionResult> ImportFeeHistory([FromBody] List<FeeRow> rows)
        {
            var res = new ImportResult();
            if (rows == null || rows.Count == 0) return Ok(res);

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                var reg = S(r.StudentId);
                if (reg.Length == 0) { res.Errors.Add($"Row {i + 2}: missing StudentID"); continue; }

                try
                {
                    var student = await db.Students.FirstOrDefaultAsync(s => s.RegistrationNo == reg);
                    if (student == null) { res.Errors.Add($"Row {i + 2}: student {reg} not found"); continue; }

                    var admission = await db.Admissions.Where(a => a.StudentID == student.StudentID)
                        .OrderByDescending(a => a.IsActive).ThenByDescending(a => a.RegistrationDate)
                        .FirstOrDefaultAsync();
                    if (admission == null) { res.Errors.Add($"Row {i + 2}: no admission for {reg}"); continue; }

                    var receipt = S(r.ReceiptNo);
                    // Idempotency: skip if a payment with this receipt already exists for the student
                    if (receipt.Length > 0 &&
                        await db.Payments.AnyAsync(p => p.StudentId == student.StudentID && p.Remarks == receipt))
                    { res.Skipped++; continue; }

                    var mNum = MonthNum(r.Month);
                    var yr = r.Year ?? DateTime.Now.Year;
                    DateTime? feeMonth = mNum >= 1 ? new DateTime(yr, mNum, 1) : null;
                    var paidOn = Date(r.PaidDate) ?? DateTime.Now;

                    var dueIds = new List<(Guid id, decimal amt)>();

                    void AddDue(FeeDueType type, decimal baseAmt, decimal lateAmt, DateTime? fm)
                    {
                        if (baseAmt <= 0 && lateAmt <= 0) return;
                        var due = new FeeDue
                        {
                            FeeDueId = Guid.NewGuid(),
                            AdmissionId = admission.AdmissionID,
                            FeeType = type,
                            FeeMonth = type == FeeDueType.Monthly ? fm : null,
                            BaseAmount = baseAmt,
                            LateFeeAmount = lateAmt,
                            DueDate = fm ?? paidOn,
                            IsLateFeeWaived = false,
                            Status = FeeDueStatus.Paid,
                            CreatedAt = DateTime.Now,
                        };
                        db.FeeDues.Add(due);
                        dueIds.Add((due.FeeDueId, baseAmt + lateAmt));
                    }

                    AddDue(FeeDueType.Monthly, Dec(r.MonthlyFee), Dec(r.LateFee), feeMonth);
                    AddDue(FeeDueType.Admission, Dec(r.AdmissionAmount), 0, null);
                    AddDue(FeeDueType.Card, Dec(r.CardAmount), 0, null);

                    if (dueIds.Count == 0) { res.Skipped++; continue; }

                    var total = r.Total ?? dueIds.Sum(x => x.amt);
                    var payment = new Payment
                    {
                        PaymentId = Guid.NewGuid(),
                        StudentId = student.StudentID,
                        TotalAmount = total,
                        PaymentDate = paidOn,
                        PaymentMethod = PaymentMethod.Cash,
                        Remarks = receipt.Length > 0 ? receipt : null,
                    };
                    db.Payments.Add(payment);
                    foreach (var (id, amt) in dueIds)
                        db.PaymentDetails.Add(new PaymentDetail { PaymentDetailId = Guid.NewGuid(), PaymentId = payment.PaymentId, FeeDueId = id, PaidAmount = amt });

                    await db.SaveChangesAsync();
                    res.Created++;
                }
                catch (Exception ex)
                {
                    res.Errors.Add($"Row {i + 2} ({reg}): {ex.Message}");
                }
            }
            return Ok(res);
        }

        // ════════ TEACHERS (flexible) ════════
        public class TeacherRow
        {
            public string? Name { get; set; }
            public string? FatherName { get; set; }
            public string? Gender { get; set; }
            public string? Dob { get; set; }
            public string? Contact { get; set; }
            public string? EmergencyContact { get; set; }
            public string? Cnic { get; set; }
            public string? Qualification { get; set; }
            public string? Institute { get; set; }
            public string? Experience { get; set; }
            public string? Occupation { get; set; }
            public string? Region { get; set; }
            public string? City { get; set; }
            public string? Address { get; set; }
            public string? RegDate { get; set; }
        }

        [HttpPost("teachers")]
        public async Task<IActionResult> ImportTeachers([FromBody] List<TeacherRow> rows)
        {
            var res = new ImportResult();
            if (rows == null || rows.Count == 0) return Ok(res);

            var maxSerial = await db.Teachers.MaxAsync(t => (int?)t.Serial) ?? 0;
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                var name = S(r.Name);
                if (name.Length == 0) { res.Errors.Add($"Row {i + 2}: missing Name"); continue; }
                var father = S(r.FatherName);
                if (await db.Teachers.AnyAsync(t => t.TeacherName == name && t.FatherName == father)) { res.Skipped++; continue; }

                try
                {
                    var serial = ++maxSerial;
                    var regDate = Date(r.RegDate) ?? DateTime.Now;
                    var teacher = new Teachers
                    {
                        TeacherID = Guid.NewGuid(),
                        Serial = serial,
                        RegistrationNo = $"LT-{regDate:MMMyy}-{serial:D3}",
                        TeacherName = name,
                        FatherName = father,
                        Gender = S(r.Gender).Length > 0 ? S(r.Gender) : "—",
                        DateOfBirth = Date(r.Dob) ?? new DateTime(1900, 1, 1),
                        Address = S(r.Address),
                        City = S(r.City),
                        Region = S(r.Region),
                        EmergencyContact = S(r.EmergencyContact),
                        Contact = S(r.Contact),
                        FatherOccupation = S(r.Occupation),
                        Qualification = S(r.Qualification),
                        Institute = S(r.Institute),
                        Cnic = S(r.Cnic),
                        Picture = "",
                        Experience = S(r.Experience),
                        RegistrationDate = regDate,
                        IsTeaching = true,
                    };
                    db.Teachers.Add(teacher);
                    await db.SaveChangesAsync();
                    res.Created++;
                }
                catch (Exception ex) { res.Errors.Add($"Row {i + 2} ({name}): {ex.Message}"); }
            }
            return Ok(res);
        }

        // ════════ PHOTO (matched by RegNo) ════════
        [HttpPost("photo")]
        public async Task<IActionResult> ImportPhoto([FromForm] string regNo, IFormFile file)
        {
            regNo = S(regNo);
            if (string.IsNullOrWhiteSpace(regNo) || file == null || file.Length == 0)
                return BadRequest(new { message = "regNo and file are required." });

            var student = await db.Students.FirstOrDefaultAsync(s => s.RegistrationNo == regNo);
            if (student == null) return Ok(new { matched = false });

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
            var dir = Path.Combine(env.ContentRootPath, "Images", "Students");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"{regNo}{ext}");
            using (var fs = new FileStream(path, FileMode.Create)) await file.CopyToAsync(fs);

            var req = http.HttpContext!.Request;
            student.Picture = $"{req.Scheme}://{req.Host}{req.PathBase}/Images/Students/{regNo}{ext}";
            await db.SaveChangesAsync();
            return Ok(new { matched = true });
        }
    }
}
