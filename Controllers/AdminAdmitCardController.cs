using InstituteWebAPI.Data;
using InstituteWebAPI.Services.TermContext;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminAdmitCardController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly ITermContext termContext;

        public AdminAdmitCardController(RozhnInstituteDbContext dbContext, ITermContext termContext)
        {
            this.dbContext = dbContext;
            this.termContext = termContext;
        }

        // ── DTOs ──────────────────────────────────────────────────────────────
        public class PendingTestDto
        {
            public string? Title { get; set; }
            public DateTime ScheduledOn { get; set; }
            public string? TeacherName { get; set; }
            public string? Month { get; set; }
        }

        public class AdmitCardStudentDto
        {
            public Guid StudentID { get; set; }
            public string? RegistrationNo { get; set; }
            public string? StudentName { get; set; }
            public string? FatherName { get; set; }
            public string? Picture { get; set; }

            public Guid CurrentClassID { get; set; }
            public string? ClassName { get; set; }
            public string? CourseName { get; set; }
            public string? TeacherName { get; set; }
            public string? SlotName { get; set; }
            public string? SectionName { get; set; }

            public int UnpaidMonths { get; set; }
            public List<PendingTestDto> PendingTests { get; set; } = new();
        }

        public class AdmitCardRequestDto
        {
            [Required, MinLength(1)]
            public List<Guid> CurrentClassIDs { get; set; } = new();

            /// <summary>Include students with unpaid months &lt;= this value.</summary>
            [Range(0, 60)]
            public int MaxUnpaidMonths { get; set; }

            /// <summary>Optional subset of students to generate for. Empty = all matching.</summary>
            public List<Guid> StudentIDs { get; set; } = new();
        }

        // ── GET classes (active term) ─────────────────────────────────────────
        [HttpGet("classes")]
        public async Task<IActionResult> GetClasses()
        {
            var activeTerm = await termContext.GetActiveTermAsync();
            var classes = await dbContext.CurrentClasses
                .AsNoTracking()
                .Include(cc => cc.Class).ThenInclude(c => c.Course)
                .Include(cc => cc.Teacher)
                .Include(cc => cc.Slot)
                .Include(cc => cc.Section)
                .Where(cc => cc.TermID == activeTerm.TermID)
                .OrderBy(cc => cc.Class.ClassName)
                .Select(cc => new
                {
                    cc.CurrentClassID,
                    ClassName   = cc.Class.ClassName,
                    CourseName  = cc.Class.Course != null ? cc.Class.Course.CourseName : null,
                    TeacherName = cc.Teacher != null ? cc.Teacher.TeacherName : null,
                    SlotName    = cc.Slot != null ? cc.Slot.SlotName : null,
                    SectionName = cc.Section != null ? cc.Section.Name : null,
                })
                .ToListAsync();

            return Ok(classes);
        }

        // ── POST preview ──────────────────────────────────────────────────────
        [HttpPost("preview")]
        public async Task<IActionResult> Preview([FromBody] AdmitCardRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var rows = await BuildStudentsAsync(dto);
            return Ok(rows);
        }

        // ── POST generate (store + return data for PDF) ───────────────────────
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] AdmitCardRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var rows = await BuildStudentsAsync(dto);

            // Optional student subset
            if (dto.StudentIDs is { Count: > 0 })
            {
                var set = new HashSet<Guid>(dto.StudentIDs);
                rows = rows.Where(r => set.Contains(r.StudentID)).ToList();
            }

            var activeTerm = await termContext.GetActiveTermAsync();
            var now = DateTime.UtcNow;

            foreach (var r in rows)
            {
                dbContext.AdmitCards.Add(new AdmitCard
                {
                    AdmitCardID    = Guid.NewGuid(),
                    StudentID      = r.StudentID,
                    CurrentClassID = r.CurrentClassID,
                    TermID         = activeTerm.TermID,
                    UnpaidMonths   = r.UnpaidMonths,
                    GeneratedOn    = now,
                });
            }
            await dbContext.SaveChangesAsync();

            return Ok(rows);
        }

        // ── Core builder ──────────────────────────────────────────────────────
        private async Task<List<AdmitCardStudentDto>> BuildStudentsAsync(AdmitCardRequestDto dto)
        {
            var activeTerm = await termContext.GetActiveTermAsync();
            var classIds = dto.CurrentClassIDs.Distinct().ToList();

            // Classes (must be in active term)
            var classes = await dbContext.CurrentClasses
                .AsNoTracking()
                .Include(cc => cc.Class).ThenInclude(c => c.Course)
                .Include(cc => cc.Teacher)
                .Include(cc => cc.Slot)
                .Include(cc => cc.Section)
                .Where(cc => classIds.Contains(cc.CurrentClassID) && cc.TermID == activeTerm.TermID)
                .ToListAsync();

            var validClassIds = classes.Select(c => c.CurrentClassID).ToList();

            // Enrolled students in those classes
            var enrolments = await dbContext.ClassStudents
                .AsNoTracking()
                .Include(cs => cs.Student)
                .Where(cs => validClassIds.Contains(cs.CurrentClassID) && cs.Status == "Enrolled")
                .ToListAsync();

            var studentIds = enrolments.Select(e => e.StudentID).Distinct().ToList();

            // Unpaid monthly-fee counts per student (Status != Paid)
            var unpaidByStudent = await dbContext.FeeDues
                .AsNoTracking()
                .Where(d => d.FeeType == FeeDueType.Monthly
                            && d.Status != FeeDueStatus.Paid
                            && studentIds.Contains(d.Admission.StudentID))
                .GroupBy(d => d.Admission.StudentID)
                .Select(g => new { StudentID = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StudentID, x => x.Count);

            // Month label map: TermMonths ordered by TermMonth -> "Month 1/2/3"
            var orderedMonths = await dbContext.TermMonths
                .AsNoTracking()
                .OrderBy(m => m.TermMonth)
                .Select(m => m.TermMonthID)
                .ToListAsync();
            var monthLabel = new Dictionary<Guid, string>();
            for (int idx = 0; idx < orderedMonths.Count; idx++)
                monthLabel[orderedMonths[idx]] = $"Month {idx + 1}";

            // Pending (Coming) scheduled tests per class
            var pendingTests = await dbContext.TestSchedules
                .AsNoTracking()
                .Where(t => validClassIds.Contains(t.CurrentClassID) && t.Status == "Coming")
                .OrderBy(t => t.ScheduledOn)
                .Select(t => new { t.CurrentClassID, t.Title, t.ScheduledOn, t.TermMonthID })
                .ToListAsync();

            var classById = classes.ToDictionary(c => c.CurrentClassID, c => c);
            var testsByClass = pendingTests
                .GroupBy(t => t.CurrentClassID)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<AdmitCardStudentDto>();

            foreach (var cs in enrolments)
            {
                if (!classById.TryGetValue(cs.CurrentClassID, out var cc)) continue;

                var unpaid = unpaidByStudent.TryGetValue(cs.StudentID, out var u) ? u : 0;
                if (unpaid > dto.MaxUnpaidMonths) continue;  // filter

                var teacherName = cc.Teacher?.TeacherName;
                var tests = testsByClass.TryGetValue(cs.CurrentClassID, out var tl)
                    ? tl.Select(t => new PendingTestDto
                      {
                          Title = t.Title,
                          ScheduledOn = t.ScheduledOn,
                          TeacherName = teacherName,
                          Month = t.TermMonthID.HasValue && monthLabel.TryGetValue(t.TermMonthID.Value, out var ml) ? ml : null,
                      }).ToList()
                    : new List<PendingTestDto>();

                result.Add(new AdmitCardStudentDto
                {
                    StudentID      = cs.StudentID,
                    RegistrationNo = cs.Student?.RegistrationNo,
                    StudentName    = cs.Student?.StudentName,
                    FatherName     = cs.Student?.FatherName,
                    Picture        = cs.Student?.Picture,
                    CurrentClassID = cs.CurrentClassID,
                    ClassName      = cc.Class?.ClassName,
                    CourseName     = cc.Class?.Course?.CourseName,
                    TeacherName    = teacherName,
                    SlotName       = cc.Slot?.SlotName,
                    SectionName    = cc.Section?.Name,
                    UnpaidMonths   = unpaid,
                    PendingTests   = tests,
                });
            }

            return result
                .OrderBy(r => r.ClassName)
                .ThenBy(r => r.StudentName)
                .ToList();
        }
    }
}
