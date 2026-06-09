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
    public class AdminTestScheduleController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly ITermContext termContext;

        public AdminTestScheduleController(RozhnInstituteDbContext dbContext, ITermContext termContext)
        {
            this.dbContext = dbContext;
            this.termContext = termContext;
        }

        private static readonly HashSet<string> AllowedStatuses =
            new(StringComparer.OrdinalIgnoreCase) { "Coming", "Conducted", "Cancelled", "Postponed" };

        // ── DTOs ──────────────────────────────────────────────────────────────
        public class TestScheduleDto
        {
            public Guid TestScheduleID { get; set; }
            public Guid CurrentClassID { get; set; }
            public string? ClassName { get; set; }
            public string? CourseName { get; set; }
            public string? TeacherName { get; set; }
            public Guid? TermMonthID { get; set; }
            public int? TermMonth { get; set; }
            public string? Title { get; set; }
            public DateTime ScheduledOn { get; set; }
            public string Status { get; set; } = "Coming";
            public string? Notes { get; set; }
        }

        public class SaveTestScheduleDto
        {
            [Required]
            public Guid CurrentClassID { get; set; }

            public Guid? TermMonthID { get; set; }

            public string? Title { get; set; }

            [Required]
            public DateTime ScheduledOn { get; set; }

            public string? Status { get; set; }

            public string? Notes { get; set; }
        }

        public class CreateTestScheduleDto
        {
            /// <summary>One or more classes the test is scheduled for.</summary>
            [Required, MinLength(1)]
            public List<Guid> CurrentClassIDs { get; set; } = new();

            public Guid? TermMonthID { get; set; }

            public string? Title { get; set; }

            [Required]
            public DateTime ScheduledOn { get; set; }

            public string? Status { get; set; }

            public string? Notes { get; set; }
        }

        // ── GET classes available for scheduling (active term) ────────────────
        [HttpGet("classes")]
        public async Task<IActionResult> GetClasses()
        {
            var activeTerm = await termContext.GetActiveTermAsync();
            var classes = await dbContext.CurrentClasses
                .AsNoTracking()
                .Include(cc => cc.Class).ThenInclude(c => c.Course)
                .Include(cc => cc.Teacher)
                .Where(cc => cc.TermID == activeTerm.TermID)
                .OrderBy(cc => cc.Class.ClassName)
                .Select(cc => new
                {
                    cc.CurrentClassID,
                    ClassName   = cc.Class.ClassName,
                    CourseName  = cc.Class.Course != null ? cc.Class.Course.CourseName : null,
                    TeacherName = cc.Teacher != null ? cc.Teacher.TeacherName : null,
                })
                .ToListAsync();

            return Ok(classes);
        }

        // ── GET schedules (active term, optional filters) ─────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? currentClassId, [FromQuery] string? status)
        {
            var activeTerm = await termContext.GetActiveTermAsync();

            var query = dbContext.TestSchedules
                .AsNoTracking()
                .Include(t => t.CurrentClass).ThenInclude(cc => cc.Class).ThenInclude(c => c.Course)
                .Include(t => t.CurrentClass).ThenInclude(cc => cc.Teacher)
                .Include(t => t.TermMonth)
                .Where(t => t.CurrentClass.TermID == activeTerm.TermID);

            if (currentClassId.HasValue) query = query.Where(t => t.CurrentClassID == currentClassId.Value);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(t => t.Status == status);

            var rows = await query
                .OrderBy(t => t.ScheduledOn)
                .Select(t => new TestScheduleDto
                {
                    TestScheduleID = t.TestScheduleID,
                    CurrentClassID = t.CurrentClassID,
                    ClassName      = t.CurrentClass.Class.ClassName,
                    CourseName     = t.CurrentClass.Class.Course != null ? t.CurrentClass.Class.Course.CourseName : null,
                    TeacherName    = t.CurrentClass.Teacher != null ? t.CurrentClass.Teacher.TeacherName : null,
                    TermMonthID    = t.TermMonthID,
                    TermMonth      = t.TermMonth != null ? t.TermMonth.TermMonth : (int?)null,
                    Title          = t.Title,
                    ScheduledOn    = t.ScheduledOn,
                    Status         = t.Status,
                    Notes          = t.Notes,
                })
                .ToListAsync();

            return Ok(rows);
        }

        // ── POST create (one test for one or more classes) ────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTestScheduleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var classIds = dto.CurrentClassIDs.Distinct().ToList();
            if (classIds.Count == 0)
                return BadRequest(new { message = "Select at least one class." });

            var activeTerm = await termContext.GetActiveTermAsync();

            // All selected classes must exist and belong to the active term.
            var validIds = await dbContext.CurrentClasses
                .Where(cc => classIds.Contains(cc.CurrentClassID) && cc.TermID == activeTerm.TermID)
                .Select(cc => cc.CurrentClassID)
                .ToListAsync();

            if (validIds.Count != classIds.Count)
                return BadRequest(new { message = "One or more selected classes are invalid or not in the active term." });

            var title  = string.IsNullOrWhiteSpace(dto.Title) ? null : dto.Title.Trim();
            var notes  = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
            var status = NormalizeStatus(dto.Status);

            foreach (var classId in validIds)
            {
                dbContext.TestSchedules.Add(new TestSchedule
                {
                    TestScheduleID = Guid.NewGuid(),
                    CurrentClassID = classId,
                    TermMonthID    = dto.TermMonthID,
                    Title          = title,
                    ScheduledOn    = dto.ScheduledOn,
                    Status         = status,
                    Notes          = notes,
                    CreatedOn      = DateTime.UtcNow,
                });
            }

            await dbContext.SaveChangesAsync();
            return Ok(new { Created = validIds.Count });
        }

        // ── PUT update ────────────────────────────────────────────────────────
        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SaveTestScheduleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = await dbContext.TestSchedules.FirstOrDefaultAsync(t => t.TestScheduleID == id);
            if (entity == null) return NotFound();

            var activeTerm   = await termContext.GetActiveTermAsync();
            var currentClass = await dbContext.CurrentClasses
                .FirstOrDefaultAsync(cc => cc.CurrentClassID == dto.CurrentClassID);
            if (currentClass == null) return BadRequest(new { message = "Selected class was not found." });
            if (currentClass.TermID != activeTerm.TermID)
                return BadRequest(new { message = "Tests can only be scheduled for classes in the active term." });

            entity.CurrentClassID = dto.CurrentClassID;
            entity.TermMonthID    = dto.TermMonthID;
            entity.Title          = string.IsNullOrWhiteSpace(dto.Title) ? null : dto.Title.Trim();
            entity.ScheduledOn    = dto.ScheduledOn;
            entity.Status         = NormalizeStatus(dto.Status);
            entity.Notes          = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
            entity.UpdatedOn      = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
            return Ok(new { entity.TestScheduleID });
        }

        // ── PATCH status only ─────────────────────────────────────────────────
        public class SetStatusDto { public string? Status { get; set; } }

        [HttpPut("{id:Guid}/status")]
        public async Task<IActionResult> SetStatus(Guid id, [FromBody] SetStatusDto dto)
        {
            var entity = await dbContext.TestSchedules.FirstOrDefaultAsync(t => t.TestScheduleID == id);
            if (entity == null) return NotFound();

            entity.Status = NormalizeStatus(dto.Status);
            entity.UpdatedOn = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            return Ok(new { entity.TestScheduleID, entity.Status });
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await dbContext.TestSchedules.FirstOrDefaultAsync(t => t.TestScheduleID == id);
            if (entity == null) return NotFound();

            dbContext.TestSchedules.Remove(entity);
            await dbContext.SaveChangesAsync();
            return Ok(new { id });
        }

        private static string NormalizeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "Coming";
            var match = AllowedStatuses.FirstOrDefault(s => s.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));
            return match ?? "Coming";
        }
    }
}
