using AutoMapper;
using InstituteWebAPI.Data;
using InstituteWebAPI.Models.DTO.StudentMarks;
using InstituteWebAPI.Models.DTO.StudentMarks.Terminal;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebAPI.Services.TermContext;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherStudentMarksController : ControllerBase
    {
        private readonly IStudentMarksRepository repository;
        private readonly ITestsRepository testsRepository;
        private readonly ICurrentClassRepository currentClassRepository;
        private readonly ITeacherIdentityLinkRepository teacherIdentity;
        private readonly RozhnInstituteDbContext dbContext;
        private readonly IMapper mapper;
        private readonly ITermContext termContext;
        private readonly InstituteWebAPI.Services.StudentMonthlyResults.IStudentMonthlyResultService studentMonthlyResultService;

        public TeacherStudentMarksController(
            IStudentMarksRepository repository,
            ITestsRepository testsRepository,
            ICurrentClassRepository currentClassRepository,
            ITeacherIdentityLinkRepository teacherIdentity,
            RozhnInstituteDbContext dbContext,
            IMapper mapper,
            ITermContext termContext,
            InstituteWebAPI.Services.StudentMonthlyResults.IStudentMonthlyResultService studentMonthlyResultService)
        {
            this.repository = repository;
            this.testsRepository = testsRepository;
            this.currentClassRepository = currentClassRepository;
            this.teacherIdentity = teacherIdentity;
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.termContext = termContext;
            this.studentMonthlyResultService = studentMonthlyResultService;
        }

        private async Task<Guid?> GetTeacherIdFromTokenAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return null;
            return await teacherIdentity.GetTeacherIdForUserIdAsync(userId);
        }

        private async Task<bool> TeacherOwnsCurrentClass(Guid currentClassId)
        {
            var currentClass = await currentClassRepository.GetAsync(currentClassId);
            if (currentClass == null) return false;

            var teacherIdFromToken = await GetTeacherIdFromTokenAsync();
            if (teacherIdFromToken == null) return false;

            return currentClass.TeacherID == teacherIdFromToken;
        }

        private async Task<bool> TeacherOwnsTest(Guid testId)
        {
            var test = await testsRepository.GetAsync(testId);
            if (test == null) return false;

            if (test.CurrentClassID == null) return false;

            return await TeacherOwnsCurrentClass(test.CurrentClassID);
        }

        // ---------------- Existing endpoints ----------------

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var studentMarks = await repository.GetAllAsync();
            return Ok(mapper.Map<List<StudentMarksDto>>(studentMarks));
        }

        [HttpGet("{id:Guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var studentMarks = await repository.GetAsync(id);
            if (studentMarks == null) return NotFound();

            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsTest(studentMarks.TestID);
                if (!owns) return Forbid();
            }

            return Ok(mapper.Map<StudentMarksDto>(studentMarks));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Create(AddStudentMarksDto dto)
        {
            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsTest(dto.TestID);
                if (!owns) return Forbid();
            }

            var studentMarks = mapper.Map<StudentMarks>(dto);
            studentMarks = await repository.AddAsync(studentMarks);
            return Ok(mapper.Map<StudentMarksDto>(studentMarks));
        }

        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Update(Guid id, UpdateStudentMarksDto dto)
        {
            if (User.IsInRole("Teacher"))
            {
                var existing = await repository.GetAsync(id);
                if (existing == null) return NotFound();

                var ownsExisting = await TeacherOwnsTest(existing.TestID);
                if (!ownsExisting) return Forbid();

                var ownsNew = await TeacherOwnsTest(dto.TestID);
                if (!ownsNew) return Forbid();
            }

            var updated = mapper.Map<StudentMarks>(dto);
            updated = await repository.UpdateAsync(id, updated);

            if (updated == null) return NotFound();
            return Ok(mapper.Map<StudentMarksDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (User.IsInRole("Teacher"))
            {
                var existing = await repository.GetAsync(id);
                if (existing == null) return NotFound();

                var owns = await TeacherOwnsTest(existing.TestID);
                if (!owns) return Forbid();
            }

            var deleted = await repository.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<StudentMarksDto>(deleted));
        }

        [HttpGet("search")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Search([FromQuery] Guid? studentId, [FromQuery] Guid? testId)
        {
            if (User.IsInRole("Teacher"))
            {
                if (!testId.HasValue)
                {
                    return BadRequest("testId is required for teachers.");
                }

                var owns = await TeacherOwnsTest(testId.Value);
                if (!owns) return Forbid();
            }

            List<StudentMarks>? result = null;

            if (studentId.HasValue)
                result = await repository.GetByStudentIdAsync(studentId.Value);
            else if (testId.HasValue)
                result = await repository.GetByTestIdAsync(testId.Value);

            if (result == null || result.Count == 0) return NotFound();

            return Ok(mapper.Map<List<StudentMarksDto>>(result));
        }

        // ---------------- New endpoints for bulk/monthly workflow ----------------

        // Get enrolled students of a class (teacher-scoped)
        [HttpGet("class-students")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetClassStudents([FromQuery] Guid currentClassId)
        {
            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(currentClassId);
                if (!owns) return Forbid();
            }

            var students = await dbContext.ClassStudents
                //.AsNoTracking()
                .Include(cs => cs.Student)
                .Where(cs => cs.CurrentClassID == currentClassId && cs.Status == "Enrolled")
                .Select(cs => new ClassStudentLookupDto
                {
                    StudentID = cs.StudentID,
                    RegistrationNo = cs.Student.RegistrationNo,
                    StudentName = cs.Student.StudentName,
                    FatherName = cs.Student.FatherName
                })
                .OrderBy(s => s.StudentName)
                .ToListAsync();

            return Ok(students);
        }

        // Bulk add marks for a single test (teacher-scoped) with duplicate protection
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> BulkCreate([FromBody] BulkAddStudentMarksDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (User.IsInRole("Teacher"))
            {
                var ownsClass = await TeacherOwnsCurrentClass(dto.CurrentClassID);
                if (!ownsClass) return Forbid();

                var ownsTest = await TeacherOwnsTest(dto.TestID);
                if (!ownsTest) return Forbid();
            }

            // Verify test belongs to the selected class + month
            var test = await testsRepository.GetAsync(dto.TestID);
            if (test == null) return NotFound("Test not found");
            if (test.CurrentClassID != dto.CurrentClassID) return BadRequest("Test does not belong to selected class.");
            if (test.TermMonthID != dto.TermMonthID) return BadRequest("Test does not belong to selected month.");

            var studentIds = dto.Items.Select(i => i.StudentID).Distinct().ToList();

            // Ensure students are in the class (enrolled)
            var allowedStudents = await dbContext.ClassStudents
                .AsNoTracking()
                .Where(cs => cs.CurrentClassID == dto.CurrentClassID && cs.Status == "Enrolled" && studentIds.Contains(cs.StudentID))
                .Select(cs => cs.StudentID)
                .ToListAsync();

            if (allowedStudents.Count != studentIds.Count)
            {
                return BadRequest("One or more students are not enrolled in the selected class.");
            }

            // Duplicate protection: StudentMarks already exists for (StudentID, TestID)
            var existing = await dbContext.StudentMarks
                .AsNoTracking()
                .Where(sm => sm.TestID == dto.TestID && studentIds.Contains(sm.StudentID))
                .Select(sm => sm.StudentID)
                .ToListAsync();

            if (existing.Any())
            {
                return BadRequest($"Marks already exist for {existing.Count} student(s) for this test.");
            }

            var entities = dto.Items.Select(i =>
            {
                var pct = test.TotalMarks <= 0 ? 0f : (i.ObtainedMarks / test.TotalMarks * 100f);

                return new StudentMarks
                {
                    StudentMarkID = Guid.NewGuid(),
                    TestID = dto.TestID,
                    StudentID = i.StudentID,
                    TermID = dto.TermID,
                    ObtainedMarks = i.ObtainedMarks,
                    TotalMarks = test.TotalMarks,
                    Percentage = pct
                };
            }).ToList();

            await dbContext.StudentMarks.AddRangeAsync(entities);
            await dbContext.SaveChangesAsync();

            // persist monthly aggregates
            await studentMonthlyResultService.RecalculateAsync(dto.TermID, dto.CurrentClassID, dto.TermMonthID, dto.Items.Select(x => x.StudentID));

            return Ok("Marks added successfully.");
        }

        // Monthly result sheet by class + month (teacher-scoped)
        [HttpGet("monthly-result")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> MonthlyResult([FromQuery] Guid currentClassId, [FromQuery] Guid termMonthId)
        {
            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(currentClassId);
                if (!owns) return Forbid();
            }

            var tests = await dbContext.Tests
                // .AsNoTracking() // do not disable tracking: needed for bytea in-memory comparison
                .Where(t => t.CurrentClassID == currentClassId && t.TermMonthID == termMonthId)
                .Select(t => new MonthlyTestHeaderDto
                {
                    TestID = t.TestID,
                    TestType = t.TestType,
                    TotalMarks = t.TotalMarks
                })
                .OrderBy(t => t.TestType)
                .ToListAsync();

            var totalMarks = tests.Sum(t => t.TotalMarks);

            // Passing marks are now scoped per (TermID, CurrentClassID, TermMonthID).
            // We can get TermID from CurrentClass.
            var currentClass = await dbContext.CurrentClasses.AsNoTracking().FirstOrDefaultAsync(c => c.CurrentClassID == currentClassId);
            if (currentClass == null) return NotFound("Class not found");

            var passing = await dbContext.TermMonthPassingMarks
                .AsNoTracking()
                .Where(p => p.CurrentClassID == currentClassId && p.TermID == currentClass.TermID)
                .Select(p => (float?)p.PassingMarks)
                .MaxAsync() ?? 0;

            var classStudents = await dbContext.ClassStudents
                .AsNoTracking()
                .Include(cs => cs.Student)
                .Where(cs => cs.CurrentClassID == currentClassId && cs.Status == "Enrolled")
                .Select(cs => cs.Student)
                .OrderBy(s => s.StudentName)
                .ToListAsync();

            var testIds = tests.Select(t => t.TestID).ToList();
            var studentIds = classStudents.Select(s => s.StudentID).ToList();

            var marks = await dbContext.StudentMarks
                .AsNoTracking()
                .Where(sm => testIds.Contains(sm.TestID) && studentIds.Contains(sm.StudentID))
                .Select(sm => new { sm.StudentID, sm.TestID, sm.ObtainedMarks })
                .ToListAsync();

            var marksLookup = marks
                .GroupBy(x => x.StudentID)
                .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.TestID, x => (float?)x.ObtainedMarks));

            string GradeFromPercentage(float pct) => pct switch
            {
                >= 80 => "A",
                >= 70 => "B",
                >= 60 => "C",
                >= 50 => "D",
                >= 45 => "E",
                _ => "F"
            };

            // Accumulated percentage: average of each term's percentage for this student (like SUMIFS / COUNTIF in excel)
            // We compute: for each student -> group StudentMarks by TermID -> compute termPercent = sum(Obtained)/sum(Total) * 100
            // then accumulated = average(termPercent)
            var allTestsTotals = await dbContext.Tests
                .AsNoTracking()
                .Where(t => t.CurrentClassID != null) // term comes from StudentMarks.TermID, totals from tests
                .Select(t => new { t.TestID, t.TotalMarks })
                .ToDictionaryAsync(x => x.TestID, x => x.TotalMarks);

            var allStudentMarks = await dbContext.StudentMarks
                .AsNoTracking()
                .Where(sm => studentIds.Contains(sm.StudentID))
                .Select(sm => new { sm.StudentID, sm.TermID, sm.TestID, sm.ObtainedMarks })
                .ToListAsync();

            var accumulatedLookup = allStudentMarks
                .GroupBy(sm => sm.StudentID)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var perTerm = g
                            .GroupBy(x => x.TermID)
                            .Select(termGroup =>
                            {
                                var obtained = termGroup.Sum(x => x.ObtainedMarks);
                                var total = termGroup.Sum(x => allTestsTotals.TryGetValue(x.TestID, out var tm) ? tm : 0);
                                if (total <= 0) return (float?)null;
                                return (float?)(obtained / total * 100f);
                            })
                            .Where(x => x.HasValue)
                            .Select(x => x!.Value)
                            .ToList();

                        if (perTerm.Count == 0) return (float?)null;
                        return perTerm.Average();
                    });

            var rows = classStudents.Select(s =>
            {
                var map = marksLookup.TryGetValue(s.StudentID, out var m) ? m : new Dictionary<Guid, float?>();
                var obtained = tests.Sum(t => map.TryGetValue(t.TestID, out var v) ? (v ?? 0) : 0);

                var pct = totalMarks <= 0 ? 0 : (obtained / totalMarks * 100f);

                accumulatedLookup.TryGetValue(s.StudentID, out var acc);

                return new MonthlyStudentResultRowDto
                {
                    StudentID = s.StudentID,
                    RegistrationNo = s.RegistrationNo,
                    StudentName = s.StudentName,
                    MarksByTest = tests.ToDictionary(t => t.TestID, t => map.TryGetValue(t.TestID, out var v) ? v : null),
                    ObtainedTotal = obtained,
                    Percentage = pct,
                    AccumulatedPercentage = acc,
                    Grade = totalMarks <= 0 ? "N/A" : GradeFromPercentage(pct),
                    Result = "",
                    ModifiedResult = null
                };
            }).ToList();

            // Determine pass/fail first (needed for positions)
            foreach (var r in rows)
            {
                r.Result = r.ObtainedTotal >= passing ? "Pass" : "Fail";
            }

            // Position holders: only among passed students
            var passed = rows.Where(r => r.Result == "Pass").OrderByDescending(r => r.Percentage).ThenBy(r => r.StudentName).ToList();
            for (var i = 0; i < passed.Count; i++)
            {
                if (i == 0) passed[i].Result = "1st";
                else if (i == 1) passed[i].Result = "2nd";
                else if (i == 2) passed[i].Result = "3rd";
            }

            // ModifiedResult placeholder (promoted workflow):
            // If later you store promotion decisions, set ModifiedResult = "Promoted" and overwrite Result.
            foreach (var r in rows)
            {
                if (string.Equals(r.ModifiedResult, "Promoted", StringComparison.OrdinalIgnoreCase))
                {
                    r.Result = "Promoted";
                }
            }

            var dto = new MonthlyResultDto
            {
                CurrentClassID = currentClassId,
                TermMonthID = termMonthId,
                PassingMarks = passing,
                TotalMarks = totalMarks,
                Tests = tests,
                Students = rows.OrderByDescending(r => r.Percentage).ThenBy(r => r.StudentName).ToList()
            };

            return Ok(dto);
        }

        // Get enrolled students + existing marks for a specific test (for modify screen)
        [HttpGet("test-marks")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetTestMarks([FromQuery] Guid testId)
        {
            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsTest(testId);
                if (!owns) return Forbid();
            }

            var test = await dbContext.Tests
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TestID == testId);

            if (test == null) return NotFound("Test not found");
            if (test.CurrentClassID == null) return BadRequest("Test is not linked to a class.");

            var students = await dbContext.ClassStudents
                .AsNoTracking()
                .Include(cs => cs.Student)
                .Where(cs => cs.CurrentClassID == test.CurrentClassID && cs.Status == "Enrolled")
                .OrderBy(cs => cs.Student.StudentName)
                .Select(cs => new
                {
                    cs.StudentID,
                    cs.Student.RegistrationNo,
                    StudentName = cs.Student.StudentName,
                    FatherName = cs.Student.FatherName
                })
                .ToListAsync();

            var existingMarks = await dbContext.StudentMarks
                .AsNoTracking()
                .Where(sm => sm.TestID == testId)
                .Select(sm => new { sm.StudentID, sm.StudentMarkID, sm.ObtainedMarks })
                .ToListAsync();

            var lookup = existingMarks.ToDictionary(x => x.StudentID, x => x);

            var result = students.Select(s => new
            {
                s.StudentID,
                s.RegistrationNo,
                s.StudentName,
                s.FatherName,
                StudentMarkID = lookup.TryGetValue(s.StudentID, out var m) ? (Guid?)m.StudentMarkID : null,
                ObtainedMarks = lookup.TryGetValue(s.StudentID, out var m2) ? (float?)m2.ObtainedMarks : null
            });

            return Ok(new
            {
                TestID = testId,
                TestTotalMarks = test.TotalMarks,
                Students = result
            });
        }

        // Bulk upsert marks for a single test (insert missing, update existing) with validation
        [HttpPost("bulk-upsert")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> BulkUpsert([FromBody] BulkAddStudentMarksDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (User.IsInRole("Teacher"))
            {
                var ownsClass = await TeacherOwnsCurrentClass(dto.CurrentClassID);
                if (!ownsClass) return Forbid();

                var ownsTest = await TeacherOwnsTest(dto.TestID);
                if (!ownsTest) return Forbid();
            }

            var test = await testsRepository.GetAsync(dto.TestID);
            if (test == null) return NotFound("Test not found");
            if (test.CurrentClassID != dto.CurrentClassID) return BadRequest("Test does not belong to selected class.");
            if (test.TermMonthID != dto.TermMonthID) return BadRequest("Test does not belong to selected month.");

            var invalid = dto.Items.Where(i => i.ObtainedMarks < 0 || i.ObtainedMarks > test.TotalMarks).ToList();
            if (invalid.Any())
            {
                return BadRequest($"Obtained marks must be between 0 and {test.TotalMarks} for this test.");
            }

            var studentIds = dto.Items.Select(i => i.StudentID).Distinct().ToList();

            var allowedStudents = await dbContext.ClassStudents
                .AsNoTracking()
                .Where(cs => cs.CurrentClassID == dto.CurrentClassID && cs.Status == "Enrolled" && studentIds.Contains(cs.StudentID))
                .Select(cs => cs.StudentID)
                .ToListAsync();

            if (allowedStudents.Count != studentIds.Count)
            {
                return BadRequest("One or more students are not enrolled in the selected class.");
            }

            var existing = await dbContext.StudentMarks
                .Where(sm => sm.TestID == dto.TestID && studentIds.Contains(sm.StudentID))
                .ToListAsync();

            var existingByStudent = existing.ToDictionary(sm => sm.StudentID, sm => sm);

            foreach (var item in dto.Items)
            {
                var pct = test.TotalMarks <= 0 ? 0f : (item.ObtainedMarks / test.TotalMarks * 100f);

                if (existingByStudent.TryGetValue(item.StudentID, out var sm))
                {
                    sm.ObtainedMarks = item.ObtainedMarks;
                    sm.TermID = dto.TermID;
                    sm.TotalMarks = test.TotalMarks;
                    sm.Percentage = pct;
                }
                else
                {
                    dbContext.StudentMarks.Add(new StudentMarks
                    {
                        StudentMarkID = Guid.NewGuid(),
                        TestID = dto.TestID,
                        StudentID = item.StudentID,
                        TermID = dto.TermID,
                        ObtainedMarks = item.ObtainedMarks,
                        TotalMarks = test.TotalMarks,
                        Percentage = pct
                    });
                }
            }

            await dbContext.SaveChangesAsync();

            // Persist monthly totals/obtained/percentage to DB for selected month.
            await studentMonthlyResultService.RecalculateAsync(dto.TermID, dto.CurrentClassID, dto.TermMonthID, studentIds);

            return Ok("Marks saved successfully.");
        }

        // ---------------- Terminal Result workflow ----------------

        // Quick check for UI: is terminal result created (settings exist) for class in active term
        [HttpGet("terminal/exists")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> TerminalExists([FromQuery] Guid currentClassId)
        {
            if (currentClassId == Guid.Empty) return BadRequest("currentClassId is required.");

            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(currentClassId);
                if (!owns) return Forbid();
            }

            var activeTerm = await termContext.GetActiveTermAsync();

            var exists = await dbContext.TerminalResults
                .AsNoTracking()
                .AnyAsync(x => x.CurrentClassID == currentClassId && x.TermID == activeTerm.TermID);

            return Ok(new { CurrentClassID = currentClassId, TermID = activeTerm.TermID, Exists = exists });
        }

        // View terminal result for active term.
        // If month test ids are not provided, infer them from saved terminal settings (TerminalResults) for the class.
        [HttpGet("terminal")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetTerminalResult([FromQuery] Guid currentClassId)
        {
            if (currentClassId == Guid.Empty) return BadRequest("currentClassId is required.");

            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(currentClassId);
                if (!owns) return Forbid();
            }

            var activeTerm = await termContext.GetActiveTermAsync();

            // determine month1/2/3 based on class months ordering (active term)
            var months = await dbContext.Tests
                .AsNoTracking()
                .Where(t => t.CurrentClassID == currentClassId && t.CurrentClass.TermID == activeTerm.TermID)
                .Select(t => t.TermMonthID)
                .Distinct()
                .Join(dbContext.TermMonths.AsNoTracking(), id => id, m => m.TermMonthID, (id, m) => new { m.TermMonthID, m.TermMonth })
                .OrderBy(x => x.TermMonth)
                .ToListAsync();

            if (months.Count < 3)
                return BadRequest("Active term must have at least 3 months (tests) for terminal result.");

            var m1MonthId = months[0].TermMonthID;
            var m2MonthId = months[1].TermMonthID;
            var m3MonthId = months[2].TermMonthID;

            var classStudents = await dbContext.ClassStudents
                .AsNoTracking()
                .Include(cs => cs.Student)
                .Where(cs => cs.CurrentClassID == currentClassId && cs.Status == "Enrolled")
                .Select(cs => cs.Student)
                .OrderBy(s => s.StudentName)
                .ToListAsync();

            var studentIds = classStudents.Select(s => s.StudentID).ToList();

            // terminal rows (include flags + stored computed fields)
            var settings = await dbContext.TerminalResults
                .AsNoTracking()
                .Where(x => x.TermID == activeTerm.TermID && x.CurrentClassID == currentClassId && studentIds.Contains(x.StudentID))
                .ToListAsync();

            if (settings.Count == 0)
                return NotFound("No terminal result created for this class.");

            var settingsByStudent = settings.ToDictionary(x => x.StudentID, x => x);

            // monthly aggregates
            var monthly = await dbContext.StudentMonthlyResults
                .AsNoTracking()
                .Where(x => x.TermID == activeTerm.TermID && x.CurrentClassID == currentClassId && studentIds.Contains(x.StudentID)
                            && (x.TermMonthID == m1MonthId || x.TermMonthID == m2MonthId || x.TermMonthID == m3MonthId))
                .Select(x => new { x.StudentID, x.TermMonthID, x.TotalMarks, x.ObtainedMarks })
                .ToListAsync();

            var monthlyByStudent = monthly
                .GroupBy(x => x.StudentID)
                .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.TermMonthID, x => new { x.TotalMarks, x.ObtainedMarks }));

            var monthTotals = monthly
                .GroupBy(x => x.TermMonthID)
                .ToDictionary(g => g.Key, g => g.First().TotalMarks);

            var month1Total = monthTotals.TryGetValue(m1MonthId, out var mt1) ? mt1 : 0;
            var month2Total = monthTotals.TryGetValue(m2MonthId, out var mt2) ? mt2 : 0;
            var month3Total = monthTotals.TryGetValue(m3MonthId, out var mt3) ? mt3 : 0;

            // Passing marks (same concept as monthly: threshold decides pass/fail before positions)
            var passing = await dbContext.TermMonthPassingMarks
                .AsNoTracking()
                .Where(p => p.CurrentClassID == currentClassId && p.TermID == activeTerm.TermID)
                .Select(p => (float?)p.PassingMarks)
                .MaxAsync() ?? 0;

            string GradeFromPercentage(float pct) => pct switch
            {
                >= 80 => "A",
                >= 70 => "B",
                >= 60 => "C",
                >= 50 => "D",
                >= 45 => "E",
                _ => "F"
            };

            var rows = classStudents.Select(s =>
            {
                var set = settingsByStudent.TryGetValue(s.StudentID, out var st) ? st : null;
                var include1 = set?.IncludeMonth1 ?? false;
                var include2 = set?.IncludeMonth2 ?? false;

                monthlyByStudent.TryGetValue(s.StudentID, out var map);
                map ??= new();

                map.TryGetValue(m1MonthId, out var m1);
                map.TryGetValue(m2MonthId, out var m2);
                map.TryGetValue(m3MonthId, out var m3);

                var include1Eff = include1 && m1 != null;
                var include2Eff = include2 && m2 != null;

                var total = (m3?.TotalMarks ?? 0) + (include1Eff ? m1!.TotalMarks : 0) + (include2Eff ? m2!.TotalMarks : 0);
                var obt = (m3?.ObtainedMarks ?? 0) + (include1Eff ? m1!.ObtainedMarks : 0) + (include2Eff ? m2!.ObtainedMarks : 0);
                var pct = total <= 0 ? 0f : (obt / total * 100f);

                return new TerminalStudentRowDto
                {
                    StudentID = s.StudentID,
                    RegistrationNo = s.RegistrationNo,
                    StudentName = s.StudentName,

                    Month1Obtained = m1?.ObtainedMarks,
                    Month1TotalMarks = m1?.TotalMarks,
                    Month2Obtained = m2?.ObtainedMarks,
                    Month2TotalMarks = m2?.TotalMarks,
                    Month3Obtained = m3?.ObtainedMarks,
                    Month3TotalMarks = m3?.TotalMarks,

                    IncludeMonth1 = include1,
                    IncludeMonth2 = include2,

                    TotalObtained = obt,
                    TotalMarksConsidered = total,
                    Percentage = pct,
                    Grade = total <= 0 ? "N/A" : GradeFromPercentage(pct),
                    Result = obt >= passing ? "Pass" : "Fail",

                    NI_Month1 = m1 == null,
                    NI_Month2 = m2 == null,
                    NI_Month3 = m3 == null
                };
            }).ToList();

            // Position holders among passed students (same as monthly: 1st/2nd/3rd)
            var passed = rows.Where(r => r.Result == "Pass")
                .OrderByDescending(r => r.Percentage)
                .ThenBy(r => r.StudentName)
                .ToList();

            for (var i = 0; i < passed.Count; i++)
            {
                if (i == 0) passed[i].Result = "1st";
                else if (i == 1) passed[i].Result = "2nd";
                else if (i == 2) passed[i].Result = "3rd";
            }

            return Ok(new TerminalResultDto
            {
                CurrentClassID = currentClassId,
                TermID = activeTerm.TermID,
                Month1TermMonthID = m1MonthId,
                Month1TotalMarks = month1Total,
                Month2TermMonthID = m2MonthId,
                Month2TotalMarks = month2Total,
                Month3TermMonthID = m3MonthId,
                Month3TotalMarks = month3Total,
                Students = rows.OrderByDescending(r => r.Percentage).ThenBy(r => r.StudentName).ToList()
            });
        }

        // Create/update (upsert) the terminal inclusion settings to make it permanent.
        [HttpPost("terminal/upsert")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpsertTerminal([FromBody] UpsertTerminalResultDto dto)
        {
            // NOTE: class-only workflow (month1/month2 optional include flags, month3 compulsory inferred)
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(dto.CurrentClassID);
                if (!owns) return Forbid();
            }

            var activeTerm = await termContext.GetActiveTermAsync();
            if (dto.TermID != activeTerm.TermID)
                return BadRequest("Terminal result can only be generated for active term.");

            // determine month1/2/3 based on class months ordering
            var months = await dbContext.Tests
                .AsNoTracking()
                .Where(t => t.CurrentClassID == dto.CurrentClassID && t.CurrentClass.TermID == activeTerm.TermID)
                .Select(t => t.TermMonthID)
                .Distinct()
                .Join(dbContext.TermMonths.AsNoTracking(), id => id, m => m.TermMonthID, (id, m) => new { m.TermMonthID, m.TermMonth })
                .OrderBy(x => x.TermMonth)
                .ToListAsync();

            if (months.Count < 3)
                return BadRequest("Active term must have at least 3 months (tests) for terminal result.");

            var m1MonthId = months[0].TermMonthID;
            var m2MonthId = months[1].TermMonthID;
            var m3MonthId = months[2].TermMonthID;

            // Ensure students are enrolled
            var studentIds = dto.Students.Select(s => s.StudentID).Distinct().ToList();
            var allowedStudents = await dbContext.ClassStudents
                .AsNoTracking()
                .Where(cs => cs.CurrentClassID == dto.CurrentClassID && cs.Status == "Enrolled" && studentIds.Contains(cs.StudentID))
                .Select(cs => cs.StudentID)
                .ToListAsync();

            if (allowedStudents.Count != studentIds.Count)
                return BadRequest("One or more students are not enrolled in the selected class.");

            // Load monthly aggregates
            var monthly = await dbContext.StudentMonthlyResults
                .AsNoTracking()
                .Where(x => x.TermID == dto.TermID && x.CurrentClassID == dto.CurrentClassID && studentIds.Contains(x.StudentID)
                            && (x.TermMonthID == m1MonthId || x.TermMonthID == m2MonthId || x.TermMonthID == m3MonthId))
                .Select(x => new { x.StudentID, x.TermMonthID, x.TotalMarks, x.ObtainedMarks })
                .ToListAsync();

            var monthlyByStudent = monthly
                .GroupBy(x => x.StudentID)
                .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.TermMonthID, x => new { x.TotalMarks, x.ObtainedMarks }));

            // Passing marks
            var passing = await dbContext.TermMonthPassingMarks
                .AsNoTracking()
                .Where(p => p.CurrentClassID == dto.CurrentClassID && p.TermID == dto.TermID)
                .Select(p => (float?)p.PassingMarks)
                .MaxAsync() ?? 0;

            var existing = await dbContext.TerminalResults
                .Where(x => x.TermID == dto.TermID && x.CurrentClassID == dto.CurrentClassID && studentIds.Contains(x.StudentID))
                .ToListAsync();

            var existingByStudent = existing.ToDictionary(x => x.StudentID, x => x);

            var computed = new List<(Guid StudentID, float Pct, string Result)>();

            foreach (var s in dto.Students)
            {
                monthlyByStudent.TryGetValue(s.StudentID, out var map);
                map ??= new();

                map.TryGetValue(m1MonthId, out var m1);
                map.TryGetValue(m2MonthId, out var m2);
                map.TryGetValue(m3MonthId, out var m3);

                var m3Total = m3?.TotalMarks ?? 0;
                var m3Obt = m3?.ObtainedMarks ?? 0;

                var include1 = s.IncludeMonth1 && m1 != null;
                var include2 = s.IncludeMonth2 && m2 != null;

                var total = m3Total + (include1 ? m1!.TotalMarks : 0) + (include2 ? m2!.TotalMarks : 0);
                var obt = m3Obt + (include1 ? m1!.ObtainedMarks : 0) + (include2 ? m2!.ObtainedMarks : 0);
                var pct = total <= 0 ? 0f : (obt / total * 100f);

                var res = obt >= passing ? "Pass" : "Fail";
                computed.Add((s.StudentID, pct, res));

                if (existingByStudent.TryGetValue(s.StudentID, out var row))
                {
                    row.IncludeMonth1 = include1;
                    row.IncludeMonth2 = include2;
                    row.TotalMarksConsidered = total;
                    row.TotalObtained = obt;
                    row.Percentage = pct;
                    row.Result = res;
                    row.UpdatedOn = DateTime.UtcNow;
                }
                else
                {
                    dbContext.TerminalResults.Add(new TerminalResult
                    {
                        TerminalResultID = Guid.NewGuid(),
                        TermID = dto.TermID,
                        CurrentClassID = dto.CurrentClassID,
                        StudentID = s.StudentID,
                        Month3TestID = Guid.Empty,
                        Month1TestID = null,
                        Month2TestID = null,
                        IncludeMonth1 = include1,
                        IncludeMonth2 = include2,
                        TotalMarksConsidered = total,
                        TotalObtained = obt,
                        Percentage = pct,
                        Result = res,
                        CreatedOn = DateTime.UtcNow
                    });
                }
            }

            // Position holders among passed students
            var passed = computed.Where(x => x.Result == "Pass").OrderByDescending(x => x.Pct).ToList();

            void SetPos(int idx, string pos)
            {
                if (passed.Count <= idx) return;
                var id = passed[idx].StudentID;
                if (existingByStudent.TryGetValue(id, out var row)) row.Result = pos;
                var added = dbContext.TerminalResults.Local.FirstOrDefault(x => x.StudentID == id && x.TermID == dto.TermID && x.CurrentClassID == dto.CurrentClassID);
                if (added != null) added.Result = pos;
            }

            SetPos(0, "1st");
            SetPos(1, "2nd");
            SetPos(2, "3rd");

            await dbContext.SaveChangesAsync();
            return Ok("Terminal result saved.");
        }

        public class BulkUpsertStudentMarksDto
        {
            [System.ComponentModel.DataAnnotations.Required]
            public Guid CurrentClassID { get; set; }

            [System.ComponentModel.DataAnnotations.Required]
            public Guid TermMonthID { get; set; }

            [System.ComponentModel.DataAnnotations.Required]
            public Guid TestID { get; set; }

            [System.ComponentModel.DataAnnotations.Required]
            public Guid TermID { get; set; }

            [System.ComponentModel.DataAnnotations.Required]
            public List<BulkMarkItemDto> Items { get; set; } = new();
        }

        public class BulkMarkItemDto
        {
            [System.ComponentModel.DataAnnotations.Required]
            public Guid StudentID { get; set; }

            [System.ComponentModel.DataAnnotations.Required]
            public float ObtainedMarks { get; set; }
        }

        public class ClassStudentLookupDto
        {
            public Guid StudentID { get; set; }
            public string? RegistrationNo { get; set; }
            public string? StudentName { get; set; }
            public string? FatherName { get; set; }
        }

        [HttpGet("terminal/edit-by-class")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetTerminalEditByClass([FromQuery] Guid currentClassId)
        {
            if (currentClassId == Guid.Empty) return BadRequest("currentClassId is required.");

            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(currentClassId);
                if (!owns) return Forbid();
            }

            var activeTerm = await termContext.GetActiveTermAsync();

            // Determine month1/2/3 based on term-month ordering among tests in this class+term
            var months = await dbContext.Tests
                .AsNoTracking()
                .Where(t => t.CurrentClassID == currentClassId && t.CurrentClass.TermID == activeTerm.TermID)
                .Select(t => t.TermMonthID)
                .Distinct()
                .Join(dbContext.TermMonths.AsNoTracking(), id => id, m => m.TermMonthID, (id, m) => new { m.TermMonthID, m.TermMonth })
                .OrderBy(x => x.TermMonth)
                .ToListAsync();

            if (months.Count < 3)
                return NotFound("Months/tests not found for this class in active term.");

            var m1MonthId = months[0].TermMonthID;
            var m2MonthId = months[1].TermMonthID;
            var m3MonthId = months[2].TermMonthID;

            var classStudents = await dbContext.ClassStudents
                .AsNoTracking()
                .Include(cs => cs.Student)
                .Where(cs => cs.CurrentClassID == currentClassId && cs.Status == "Enrolled")
                .Select(cs => cs.Student)
                .OrderBy(s => s.StudentName)
                .ToListAsync();

            var studentIds = classStudents.Select(s => s.StudentID).ToList();

            // Existing terminal settings (include flags)
            var settings = await dbContext.TerminalResults
                .AsNoTracking()
                .Where(x => x.TermID == activeTerm.TermID && x.CurrentClassID == currentClassId && studentIds.Contains(x.StudentID))
                .ToListAsync();

            var settingsByStudent = settings.ToDictionary(x => x.StudentID, x => x);

            // Monthly aggregates from StudentMonthlyResults
            var monthly = await dbContext.StudentMonthlyResults
                .AsNoTracking()
                .Where(x => x.TermID == activeTerm.TermID && x.CurrentClassID == currentClassId && studentIds.Contains(x.StudentID)
                            && (x.TermMonthID == m1MonthId || x.TermMonthID == m2MonthId || x.TermMonthID == m3MonthId))
                .Select(x => new { x.StudentID, x.TermMonthID, x.TotalMarks, x.ObtainedMarks })
                .ToListAsync();

            var monthlyByStudent = monthly
                .GroupBy(x => x.StudentID)
                .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.TermMonthID, x => new { x.TotalMarks, x.ObtainedMarks }));

            var rows = classStudents.Select(s =>
            {
                monthlyByStudent.TryGetValue(s.StudentID, out var map);
                map ??= new();

                map.TryGetValue(m1MonthId, out var m1);
                map.TryGetValue(m2MonthId, out var m2);
                map.TryGetValue(m3MonthId, out var m3);

                var set = settingsByStudent.TryGetValue(s.StudentID, out var st) ? st : null;

                return new TerminalStudentRowDto
                {
                    StudentID = s.StudentID,
                    RegistrationNo = s.RegistrationNo,
                    StudentName = s.StudentName,

                    Month1Obtained = m1?.ObtainedMarks,
                    Month1TotalMarks = m1?.TotalMarks,

                    Month2Obtained = m2?.ObtainedMarks,
                    Month2TotalMarks = m2?.TotalMarks,

                    Month3Obtained = m3?.ObtainedMarks,
                    Month3TotalMarks = m3?.TotalMarks,

                    IncludeMonth1 = set?.IncludeMonth1 ?? false,
                    IncludeMonth2 = set?.IncludeMonth2 ?? false,

                    NI_Month1 = m1 == null,
                    NI_Month2 = m2 == null,
                    NI_Month3 = m3 == null
                };
            }).ToList();

            return Ok(new TerminalResultDto
            {
                CurrentClassID = currentClassId,
                TermID = activeTerm.TermID,
                Month1TermMonthID = m1MonthId,
                Month2TermMonthID = m2MonthId,
                Month3TermMonthID = m3MonthId,
                Students = rows
            });
        }
    }
}
