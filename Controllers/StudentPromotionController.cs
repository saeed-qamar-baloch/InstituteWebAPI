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
    public class StudentPromotionController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly InstituteWebAPI.Services.Audit.IAuditService audit;

        public StudentPromotionController(RozhnInstituteDbContext dbContext, InstituteWebAPI.Services.Audit.IAuditService audit)
        {
            this.dbContext = dbContext;
            this.audit = audit;
        }

        // ── DTOs ─────────────────────────────────────────────────────────────

        public class PromoteStudentDto
        {
            /// <summary>The ClassStudents record ID of the student's current enrolment.</summary>
            public Guid ClassStudentID { get; set; }

            /// <summary>The target CurrentClass the student should be promoted into.</summary>
            public Guid TargetCurrentClassID { get; set; }
        }

        public class BulkPromoteDto
        {
            /// <summary>CurrentClass from which all passing students will be promoted.</summary>
            public Guid SourceCurrentClassID { get; set; }

            /// <summary>CurrentClass students will be enrolled in after promotion.</summary>
            public Guid TargetCurrentClassID { get; set; }

            /// <summary>
            /// When true, only students whose TerminalResult.Result == "Pass" are promoted.
            /// When false, all Enrolled students in the source class are promoted.
            /// Defaults to true.
            /// </summary>
            public bool OnlyPassingStudents { get; set; } = true;
        }

        // ── POST api/StudentPromotion/promote ─────────────────────────────────
        // Promotes a single student: marks their current enrolment as "Promoted"
        // and creates a new "Enrolled" record in the target class.
        [HttpPost("promote")]
        public async Task<IActionResult> Promote([FromBody] PromoteStudentDto dto)
        {
            if (dto.ClassStudentID == Guid.Empty)
                return BadRequest("ClassStudentID is required.");
            if (dto.TargetCurrentClassID == Guid.Empty)
                return BadRequest("TargetCurrentClassID is required.");

            var existing = await dbContext.ClassStudents
                .FirstOrDefaultAsync(cs => cs.ClassStudentID == dto.ClassStudentID);

            if (existing == null) return NotFound("Enrolment record not found.");
            if (existing.Status != "Enrolled")
                return BadRequest($"Student is not currently Enrolled (status: {existing.Status}).");

            // Verify target class exists and is active
            var targetClass = await dbContext.CurrentClasses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CurrentClassID == dto.TargetCurrentClassID);

            if (targetClass == null) return NotFound("Target class not found.");
            if (!targetClass.IsActive) return BadRequest("Target class is not active.");

            // Guard: already enrolled in target class?
            var alreadyEnrolled = await dbContext.ClassStudents.AnyAsync(cs =>
                cs.CurrentClassID == dto.TargetCurrentClassID &&
                cs.StudentID == existing.StudentID &&
                cs.Status == "Enrolled");

            if (alreadyEnrolled)
                return Conflict("Student is already enrolled in the target class.");

            // Mark current enrolment as promoted
            existing.Status = "Promoted";

            // Create new enrolment in target class
            var newEnrolment = new ClassStudents
            {
                ClassStudentID  = Guid.NewGuid(),
                CurrentClassID  = dto.TargetCurrentClassID,
                StudentID       = existing.StudentID,
                Status          = "Enrolled"
            };

            dbContext.ClassStudents.Add(newEnrolment);
            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                OldEnrolment = new
                {
                    existing.ClassStudentID,
                    existing.CurrentClassID,
                    existing.StudentID,
                    existing.Status
                },
                NewEnrolment = new
                {
                    newEnrolment.ClassStudentID,
                    newEnrolment.CurrentClassID,
                    newEnrolment.StudentID,
                    newEnrolment.Status
                }
            });
        }

        // ── POST api/StudentPromotion/bulk-promote ────────────────────────────
        // Promotes multiple students from one class to another in one call.
        // By default only students with TerminalResult.Result == "Pass" are
        // promoted; set OnlyPassingStudents = false to promote everyone enrolled.
        [HttpPost("bulk-promote")]
        public async Task<IActionResult> BulkPromote([FromBody] BulkPromoteDto dto)
        {
            if (dto.SourceCurrentClassID == Guid.Empty)
                return BadRequest("SourceCurrentClassID is required.");
            if (dto.TargetCurrentClassID == Guid.Empty)
                return BadRequest("TargetCurrentClassID is required.");
            if (dto.SourceCurrentClassID == dto.TargetCurrentClassID)
                return BadRequest("Source and target class cannot be the same.");

            var targetClass = await dbContext.CurrentClasses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CurrentClassID == dto.TargetCurrentClassID);

            if (targetClass == null) return NotFound("Target class not found.");
            if (!targetClass.IsActive) return BadRequest("Target class is not active.");

            // Load all currently Enrolled students from source class
            var enrolments = await dbContext.ClassStudents
                .Where(cs => cs.CurrentClassID == dto.SourceCurrentClassID && cs.Status == "Enrolled")
                .ToListAsync();

            if (enrolments.Count == 0)
                return Ok(new { Message = "No enrolled students found in source class.", Promoted = 0, Skipped = 0 });

            IEnumerable<Guid> eligibleStudentIds;

            if (dto.OnlyPassingStudents)
            {
                // Find the TermID of the source class's term
                var sourceClass = await dbContext.CurrentClasses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CurrentClassID == dto.SourceCurrentClassID);

                if (sourceClass?.TermID == null)
                    return BadRequest("Source class has no associated term — cannot filter by terminal result.");

                var passingIds = await dbContext.TerminalResults
                    .AsNoTracking()
                    .Where(tr =>
                        tr.CurrentClassID == dto.SourceCurrentClassID &&
                        tr.TermID == sourceClass.TermID &&
                        tr.Result == "Pass")
                    .Select(tr => tr.StudentID)
                    .ToListAsync();

                eligibleStudentIds = enrolments
                    .Where(cs => passingIds.Contains(cs.StudentID))
                    .Select(cs => cs.StudentID);
            }
            else
            {
                eligibleStudentIds = enrolments.Select(cs => cs.StudentID);
            }

            // Exclude students already enrolled in target class
            var alreadyInTarget = await dbContext.ClassStudents
                .AsNoTracking()
                .Where(cs => cs.CurrentClassID == dto.TargetCurrentClassID && cs.Status == "Enrolled")
                .Select(cs => cs.StudentID)
                .ToListAsync();

            var alreadySet = new HashSet<Guid>(alreadyInTarget);
            var toPromote  = enrolments
                .Where(cs => eligibleStudentIds.Contains(cs.StudentID) && !alreadySet.Contains(cs.StudentID))
                .ToList();

            var skipped = enrolments.Count - toPromote.Count;

            foreach (var enrolment in toPromote)
            {
                enrolment.Status = "Promoted";

                dbContext.ClassStudents.Add(new ClassStudents
                {
                    ClassStudentID = Guid.NewGuid(),
                    CurrentClassID = dto.TargetCurrentClassID,
                    StudentID      = enrolment.StudentID,
                    Status         = "Enrolled"
                });
            }

            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                SourceCurrentClassID = dto.SourceCurrentClassID,
                TargetCurrentClassID = dto.TargetCurrentClassID,
                OnlyPassingStudents  = dto.OnlyPassingStudents,
                Promoted             = toPromote.Count,
                Skipped              = skipped,
                Message              = $"{toPromote.Count} student(s) promoted. {skipped} skipped (not passing or already enrolled)."
            });
        }

        // ── Rank-based term promotion ─────────────────────────────────────────
        // Promotes students from a source term into the most-recently-started term,
        // routing each student into the NEXT class (by Rank) within the same course.

        private static readonly HashSet<string> EligibleResults =
            new(StringComparer.OrdinalIgnoreCase) { "Pass", "1st", "2nd", "3rd", "Promoted" };

        public class PromotionRowDto
        {
            public Guid ClassStudentID { get; set; }
            public Guid StudentID { get; set; }
            public string? StudentName { get; set; }
            public string? RegistrationNo { get; set; }

            public Guid SourceCurrentClassID { get; set; }
            public Guid SourceClassID { get; set; }
            public string? SourceClassName { get; set; }
            public int SourceRank { get; set; }
            public Guid CourseID { get; set; }
            public string? CourseName { get; set; }

            public string Result { get; set; } = "";
            public bool IsResultManual { get; set; }
            public bool Eligible { get; set; }

            public Guid? NextClassID { get; set; }
            public string? NextClassName { get; set; }
            public int? NextRank { get; set; }

            // Skip | NoRank | NoTerminalResult | Graduate | Promote
            public string Action { get; set; } = "Skip";
            public string? Note { get; set; }
        }

        public class TermPromotionPreviewDto
        {
            public Guid SourceTermID { get; set; }
            public Guid TargetTermID { get; set; }
            public string? TargetTermName { get; set; }
            public int EligibleCount { get; set; }
            public int PromoteCount { get; set; }
            public int GraduateCount { get; set; }
            public int RetainCount { get; set; }
            public int SkipCount { get; set; }
            public List<PromotionRowDto> Rows { get; set; } = new();
        }

        public class ExecuteTermPromotionDto
        {
            public Guid SourceTermID { get; set; }
            /// <summary>Optional subset of ClassStudentIDs to promote. Empty = all eligible.</summary>
            public List<Guid> ClassStudentIDs { get; set; } = new();
        }

        // Resolve next class (smallest Rank greater than current, same course).
        private static (Guid? id, string? name, int? rank) NextClass(
            List<(Guid ClassID, string ClassName, Guid CourseID, int Rank)> classes,
            Guid courseId, int rank)
        {
            var next = classes
                .Where(c => c.CourseID == courseId && c.Rank > rank)
                .OrderBy(c => c.Rank)
                .Select(c => ((Guid?)c.ClassID, (string?)c.ClassName, (int?)c.Rank))
                .FirstOrDefault();
            return next;
        }

        private async Task<(Guid targetTermId, string? targetTermName,
                            List<(Guid ClassID, string ClassName, Guid CourseID, int Rank)> classes)>
            LoadPromotionContextAsync()
        {
            var targetTerm = await dbContext.Term.AsNoTracking()
                .OrderByDescending(t => t.TermStart)
                .Select(t => new { t.TermID, t.TermName })
                .FirstOrDefaultAsync();

            var classes = (await dbContext.Classes.AsNoTracking()
                .Include(c => c.Course)
                .Select(c => new { c.ClassID, c.ClassName, c.CourseID, c.Rank, CourseName = c.Course.CourseName })
                .ToListAsync())
                .Select(c => (c.ClassID, c.ClassName, c.CourseID, c.Rank))
                .ToList();

            return (targetTerm?.TermID ?? Guid.Empty, targetTerm?.TermName, classes);
        }

        private List<PromotionRowDto> BuildPromotionRows(
            List<ClassStudents> enrolments,
            Dictionary<(Guid, Guid), TerminalResult> resultLookup,
            List<(Guid ClassID, string ClassName, Guid CourseID, int Rank)> classes,
            Dictionary<Guid, string> courseNames)
        {
            var rows = new List<PromotionRowDto>();

            foreach (var cs in enrolments)
            {
                var cls       = cs.CurrentClass?.Class;
                var courseId  = cls?.CourseID ?? Guid.Empty;
                var rank      = cls?.Rank ?? 0;
                resultLookup.TryGetValue((cs.CurrentClassID, cs.StudentID), out var tr);

                var resultStr = tr?.Result ?? "";
                var eligible  = EligibleResults.Contains(resultStr);

                var row = new PromotionRowDto
                {
                    ClassStudentID       = cs.ClassStudentID,
                    StudentID            = cs.StudentID,
                    StudentName          = cs.Student?.StudentName,
                    RegistrationNo       = cs.Student?.RegistrationNo,
                    SourceCurrentClassID = cs.CurrentClassID,
                    SourceClassID        = cls?.ClassID ?? Guid.Empty,
                    SourceClassName      = cls?.ClassName,
                    SourceRank           = rank,
                    CourseID             = courseId,
                    CourseName           = courseNames.TryGetValue(courseId, out var cn) ? cn : null,
                    Result               = resultStr,
                    IsResultManual       = tr?.IsResultManual ?? false,
                    Eligible             = eligible,
                };

                if (tr == null)
                {
                    row.Action = "NoTerminalResult";
                    row.Note   = "No terminal result for this student.";
                }
                else if (resultStr.Equals("Fail", StringComparison.OrdinalIgnoreCase))
                {
                    // Failed students move into the new term but REPEAT the same class.
                    row.NextClassID   = cls?.ClassID;
                    row.NextClassName = cls?.ClassName;
                    row.NextRank      = rank;
                    row.Action        = cls?.ClassID == null ? "Skip" : "Retain";
                    row.Note          = cls?.ClassID == null
                        ? "Failed — but no class on record to repeat."
                        : $"Failed — will repeat {cls?.ClassName} in the new term.";
                }
                else if (!eligible)
                {
                    row.Action = "Skip";
                    row.Note   = $"Result \"{resultStr}\" is not promotable.";
                }
                else if (rank <= 0)
                {
                    row.Action = "NoRank";
                    row.Note   = "Current class has no rank set — cannot determine next class.";
                }
                else
                {
                    var (nid, nname, nrank) = NextClass(classes, courseId, rank);
                    if (nid == null)
                    {
                        row.Action = "Graduate";
                        row.Note   = "Top level reached — will be marked Graduated.";
                    }
                    else
                    {
                        row.NextClassID   = nid;
                        row.NextClassName = nname;
                        row.NextRank      = nrank;
                        row.Action        = "Promote";
                    }
                }

                rows.Add(row);
            }

            return rows
                .OrderBy(r => r.CourseName)
                .ThenBy(r => r.SourceRank)
                .ThenBy(r => r.StudentName)
                .ToList();
        }

        // ── GET api/StudentPromotion/preview?sourceTermId= ────────────────────
        [HttpGet("preview")]
        public async Task<IActionResult> Preview([FromQuery] Guid sourceTermId)
        {
            if (sourceTermId == Guid.Empty) return BadRequest("sourceTermId is required.");

            var (targetTermId, targetTermName, classes) = await LoadPromotionContextAsync();
            if (targetTermId == Guid.Empty) return BadRequest("No terms exist to promote into.");

            var courseNames = await dbContext.Courses.AsNoTracking()
                .ToDictionaryAsync(c => c.CourseID, c => c.CourseName);

            var enrolments = await dbContext.ClassStudents
                .AsNoTracking()
                .Include(cs => cs.Student)
                .Include(cs => cs.CurrentClass).ThenInclude(cc => cc.Class)
                .Where(cs => cs.Status == "Enrolled" && cs.CurrentClass.TermID == sourceTermId)
                .ToListAsync();

            var results = await dbContext.TerminalResults.AsNoTracking()
                .Where(tr => tr.TermID == sourceTermId)
                .ToListAsync();

            var resultLookup = results
                .GroupBy(r => (r.CurrentClassID, r.StudentID))
                .ToDictionary(g => g.Key, g => g.First());

            var rows = BuildPromotionRows(enrolments, resultLookup, classes, courseNames);

            return Ok(new TermPromotionPreviewDto
            {
                SourceTermID   = sourceTermId,
                TargetTermID   = targetTermId,
                TargetTermName = targetTermName,
                EligibleCount  = rows.Count(r => r.Eligible),
                PromoteCount   = rows.Count(r => r.Action == "Promote"),
                GraduateCount  = rows.Count(r => r.Action == "Graduate"),
                RetainCount    = rows.Count(r => r.Action == "Retain"),
                SkipCount      = rows.Count(r => r.Action is "Skip" or "NoRank" or "NoTerminalResult"),
                Rows           = rows
            });
        }

        // ── POST api/StudentPromotion/execute ─────────────────────────────────
        [HttpPost("execute")]
        public async Task<IActionResult> Execute([FromBody] ExecuteTermPromotionDto dto)
        {
            if (dto.SourceTermID == Guid.Empty) return BadRequest("SourceTermID is required.");

            var (targetTermId, targetTermName, classes) = await LoadPromotionContextAsync();
            if (targetTermId == Guid.Empty) return BadRequest("No terms exist to promote into.");
            if (targetTermId == dto.SourceTermID)
                return BadRequest("The latest term is the same as the source term. Create/start a new term first.");

            var courseNames = await dbContext.Courses.AsNoTracking()
                .ToDictionaryAsync(c => c.CourseID, c => c.CourseName);

            // Source enrolments are READ-ONLY — the previous term must be left untouched.
            var enrolments = await dbContext.ClassStudents
                .AsNoTracking()
                .Include(cs => cs.Student)
                .Include(cs => cs.CurrentClass).ThenInclude(cc => cc.Class)
                .Where(cs => cs.Status == "Enrolled" && cs.CurrentClass.TermID == dto.SourceTermID)
                .ToListAsync();

            var results = await dbContext.TerminalResults.AsNoTracking()
                .Where(tr => tr.TermID == dto.SourceTermID)
                .ToListAsync();
            var resultLookup = results
                .GroupBy(r => (r.CurrentClassID, r.StudentID))
                .ToDictionary(g => g.Key, g => g.First());

            var rows = BuildPromotionRows(enrolments, resultLookup, classes, courseNames);

            var selected = dto.ClassStudentIDs is { Count: > 0 }
                ? new HashSet<Guid>(dto.ClassStudentIDs)
                : null;

            // Existing target-term enrolments to avoid duplicates
            var targetEnrolledPairs = await dbContext.ClassStudents
                .AsNoTracking()
                .Where(cs => cs.CurrentClass.TermID == targetTermId && cs.Status == "Enrolled")
                .Select(cs => new { cs.StudentID, cs.CurrentClass.ClassID })
                .ToListAsync();
            var alreadyInTarget = new HashSet<(Guid StudentID, Guid ClassID)>(
                targetEnrolledPairs.Select(x => (x.StudentID, x.ClassID)));

            // Cache of resolved/created target CurrentClasses keyed by destination ClassID
            var targetClassCache = new Dictionary<Guid, CurrentClass>();

            async Task<CurrentClass> ResolveTargetClassAsync(Guid classId)
            {
                if (targetClassCache.TryGetValue(classId, out var cached)) return cached;

                var existing = await dbContext.CurrentClasses
                    .FirstOrDefaultAsync(cc => cc.TermID == targetTermId && cc.ClassID == classId);

                if (existing == null)
                {
                    // New class in the target term — teacher, slot and section are left empty
                    // for the admin to assign later.
                    existing = new CurrentClass
                    {
                        CurrentClassID = Guid.NewGuid(),
                        ClassID        = classId,
                        TermID         = targetTermId,
                        TeacherID      = null,
                        SlotID         = null,
                        SectionID      = null,
                        SessionID      = null,
                        IsActive       = true,
                        CreatedOn      = DateTime.UtcNow
                    };
                    dbContext.CurrentClasses.Add(existing);
                }

                targetClassCache[classId] = existing;
                return existing;
            }

            int promoted = 0, graduated = 0, retained = 0, skipped = 0, createdClasses = 0;

            foreach (var row in rows)
            {
                if (selected != null && !selected.Contains(row.ClassStudentID)) continue;

                // Top level — nothing to promote into. Previous term is left untouched.
                if (row.Action == "Graduate")
                {
                    graduated++;
                    continue;
                }

                // Promote = move to next class; Retain = repeat the same class.
                // Both create a new enrolment in NextClassID within the target term.
                var isPromote = row.Action == "Promote";
                var isRetain  = row.Action == "Retain";
                if ((!isPromote && !isRetain) || row.NextClassID == null)
                {
                    skipped++;
                    continue;
                }

                // Skip if already enrolled in the destination class for the target term
                if (alreadyInTarget.Contains((row.StudentID, row.NextClassID.Value)))
                {
                    skipped++;
                    continue;
                }

                var target = await ResolveTargetClassAsync(row.NextClassID.Value);

                // Only a NEW enrolment is created in the target term.
                // The source-term enrolment is intentionally left as-is.
                dbContext.ClassStudents.Add(new ClassStudents
                {
                    ClassStudentID = Guid.NewGuid(),
                    CurrentClassID = target.CurrentClassID,
                    StudentID      = row.StudentID,
                    Status         = "Enrolled"
                });
                alreadyInTarget.Add((row.StudentID, row.NextClassID.Value));
                if (isRetain) retained++; else promoted++;
            }

            createdClasses = targetClassCache.Values.Count(c =>
                dbContext.Entry(c).State == EntityState.Added);

            await dbContext.SaveChangesAsync();

            await audit.LogAsync("Promotion", "Students Promoted",
                $"{promoted} promoted, {retained} retained, {graduated} graduated, {skipped} skipped into {targetTermName}. {createdClasses} class(es) auto-created.",
                "Term", dto.SourceTermID.ToString());

            return Ok(new
            {
                SourceTermID    = dto.SourceTermID,
                TargetTermID    = targetTermId,
                TargetTermName  = targetTermName,
                Promoted        = promoted,
                Retained        = retained,
                Graduated       = graduated,
                Skipped         = skipped,
                ClassesCreated  = createdClasses,
                Message         = $"{promoted} promoted, {retained} retained, {graduated} graduated, {skipped} skipped. {createdClasses} class(es) auto-created in {targetTermName}."
            });
        }
    }
}
