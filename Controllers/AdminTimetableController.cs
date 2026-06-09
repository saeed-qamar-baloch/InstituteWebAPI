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
    [Authorize(Roles = "Admin,Teacher")]
    public class AdminTimetableController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly ITermContext termContext;

        public AdminTimetableController(RozhnInstituteDbContext dbContext, ITermContext termContext)
        {
            this.dbContext = dbContext;
            this.termContext = termContext;
        }

        public class TimetableEntryDto
        {
            public Guid TimetableEntryID { get; set; }
            public Guid CurrentClassID { get; set; }
            public string? ClassName { get; set; }
            public string? CourseName { get; set; }
            public string? TeacherName { get; set; }
            public Guid SlotID { get; set; }
            public string? SlotName { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public int DayOfWeek { get; set; }
            public string? Room { get; set; }
        }

        public class SaveTimetableEntryDto
        {
            [Required] public Guid CurrentClassID { get; set; }
            [Required] public Guid SlotID { get; set; }
            [Range(1, 7)] public int DayOfWeek { get; set; }
            public string? Room { get; set; }
        }

        // Slots (rows) for the grid — only this (active) term's slots
        [HttpGet("slots")]
        public async Task<IActionResult> GetSlots()
        {
            var activeTerm = await termContext.GetActiveTermAsync();
            var slots = await dbContext.Slots
                .AsNoTracking()
                .Where(s => s.TermID == activeTerm.TermID)
                .OrderBy(s => s.StartTime)
                .Select(s => new { s.SlotID, s.SlotName, s.StartTime, s.EndTime })
                .ToListAsync();
            return Ok(slots);
        }

        // Active-term classes for the picker
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
                    SectionName = cc.Section != null ? cc.Section.Name : null,
                    SlotID      = cc.SlotID,
                    SlotName    = cc.Slot != null ? cc.Slot.SlotName : null,
                    StartTime   = cc.Slot != null ? (DateTime?)cc.Slot.StartTime : null,
                    EndTime     = cc.Slot != null ? (DateTime?)cc.Slot.EndTime : null,
                })
                .ToListAsync();
            return Ok(classes);
        }

        // Whole timetable for the active term (optionally filter by class or teacher)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? currentClassId, [FromQuery] Guid? teacherId)
        {
            var activeTerm = await termContext.GetActiveTermAsync();
            var q = dbContext.TimetableEntries
                .AsNoTracking()
                .Include(t => t.CurrentClass).ThenInclude(cc => cc.Class).ThenInclude(c => c.Course)
                .Include(t => t.CurrentClass).ThenInclude(cc => cc.Teacher)
                .Include(t => t.Slot)
                .Where(t => t.CurrentClass.TermID == activeTerm.TermID);

            if (currentClassId.HasValue) q = q.Where(t => t.CurrentClassID == currentClassId.Value);
            if (teacherId.HasValue) q = q.Where(t => t.CurrentClass.TeacherID == teacherId.Value);

            var rows = await q
                .OrderBy(t => t.DayOfWeek).ThenBy(t => t.Slot.StartTime)
                .Select(t => new TimetableEntryDto
                {
                    TimetableEntryID = t.TimetableEntryID,
                    CurrentClassID = t.CurrentClassID,
                    ClassName = t.CurrentClass.Class.ClassName,
                    CourseName = t.CurrentClass.Class.Course != null ? t.CurrentClass.Class.Course.CourseName : null,
                    TeacherName = t.CurrentClass.Teacher != null ? t.CurrentClass.Teacher.TeacherName : null,
                    SlotID = t.SlotID,
                    SlotName = t.Slot.SlotName,
                    StartTime = t.Slot.StartTime,
                    EndTime = t.Slot.EndTime,
                    DayOfWeek = t.DayOfWeek,
                    Room = t.Room,
                })
                .ToListAsync();
            return Ok(rows);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] SaveTimetableEntryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var activeTerm = await termContext.GetActiveTermAsync();
            var cc = await dbContext.CurrentClasses.FirstOrDefaultAsync(x => x.CurrentClassID == dto.CurrentClassID);
            if (cc == null) return BadRequest(new { message = "Class not found." });
            if (cc.TermID != activeTerm.TermID) return BadRequest(new { message = "Class is not in the active term." });

            var dup = await dbContext.TimetableEntries.AnyAsync(t =>
                t.CurrentClassID == dto.CurrentClassID && t.SlotID == dto.SlotID && t.DayOfWeek == dto.DayOfWeek);
            if (dup) return Conflict(new { message = "This class is already scheduled in that day/slot." });

            var entity = new TimetableEntry
            {
                TimetableEntryID = Guid.NewGuid(),
                CurrentClassID = dto.CurrentClassID,
                SlotID = dto.SlotID,
                DayOfWeek = dto.DayOfWeek,
                Room = string.IsNullOrWhiteSpace(dto.Room) ? null : dto.Room.Trim(),
                CreatedOn = DateTime.UtcNow,
            };
            dbContext.TimetableEntries.Add(entity);
            await dbContext.SaveChangesAsync();
            return Ok(new { entity.TimetableEntryID });
        }

        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var e = await dbContext.TimetableEntries.FirstOrDefaultAsync(t => t.TimetableEntryID == id);
            if (e == null) return NotFound();
            dbContext.TimetableEntries.Remove(e);
            await dbContext.SaveChangesAsync();
            return Ok(new { id });
        }
    }
}
