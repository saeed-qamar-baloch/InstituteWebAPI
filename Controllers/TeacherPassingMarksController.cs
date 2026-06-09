using InstituteWebAPI.Data;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherPassingMarksController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly Repositories.IRepository.ICurrentClassRepository currentClassRepository;
        private readonly Repositories.IRepository.ITeacherIdentityLinkRepository teacherIdentity;

        public TeacherPassingMarksController(
            RozhnInstituteDbContext dbContext,
            Repositories.IRepository.ICurrentClassRepository currentClassRepository,
            Repositories.IRepository.ITeacherIdentityLinkRepository teacherIdentity)
        {
            this.dbContext = dbContext;
            this.currentClassRepository = currentClassRepository;
            this.teacherIdentity = teacherIdentity;
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

        /// <summary>Returns the TermID that belongs to the class itself.</summary>
        private async Task<Guid?> GetClassTermIdAsync(Guid currentClassId)
        {
            var cc = await currentClassRepository.GetAsync(currentClassId);
            return cc?.TermID;
        }

        public class UpsertPassingMarksDto
        {
            public Guid CurrentClassID { get; set; }
            public Guid TermMonthID { get; set; }
            public float PassingMarks { get; set; }
        }

        // Upsert passing marks for a month for one of the teacher's classes (scoped to the class's own term)
        [HttpPost("upsert")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Upsert([FromBody] UpsertPassingMarksDto dto)
        {
            if (dto.CurrentClassID == Guid.Empty) return BadRequest("CurrentClassID is required.");
            if (dto.TermMonthID == Guid.Empty) return BadRequest("TermMonthID is required.");
            if (dto.PassingMarks < 0) return BadRequest("PassingMarks must be >= 0.");

            var classTermId = await GetClassTermIdAsync(dto.CurrentClassID);
            if (classTermId == null) return NotFound("Class not found.");

            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(dto.CurrentClassID);
                if (!owns) return Forbid();
            }

            // Validate month exists
            var monthExists = await dbContext.TermMonths.AsNoTracking().AnyAsync(m => m.TermMonthID == dto.TermMonthID);
            if (!monthExists) return NotFound("Term month not found.");

            var existing = await dbContext.TermMonthPassingMarks
                .FirstOrDefaultAsync(p =>
                    p.CurrentClassID == dto.CurrentClassID &&
                    p.TermID == classTermId &&
                    p.TermMonthID == dto.TermMonthID);

            if (existing == null)
            {
                existing = new TermMonthPassingMark
                {
                    TermMonthPassingMarkID = Guid.NewGuid(),
                    TermID = classTermId.Value,
                    CurrentClassID = dto.CurrentClassID,
                    TermMonthID = dto.TermMonthID,
                    PassingMarks = dto.PassingMarks
                };

                dbContext.TermMonthPassingMarks.Add(existing);
            }
            else
            {
                existing.PassingMarks = dto.PassingMarks;
            }

            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                existing.TermMonthPassingMarkID,
                existing.TermMonthID,
                existing.PassingMarks,
                existing.CurrentClassID,
                existing.TermID
            });
        }

        // Get current passing marks for the selected month (scoped to the class's own term)
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Get([FromQuery] Guid currentClassId, [FromQuery] Guid termMonthId)
        {
            if (currentClassId == Guid.Empty) return BadRequest("currentClassId is required.");
            if (termMonthId == Guid.Empty) return BadRequest("termMonthId is required.");

            var classTermId = await GetClassTermIdAsync(currentClassId);
            if (classTermId == null) return NotFound("Class not found.");

            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(currentClassId);
                if (!owns) return Forbid();
            }

            var passing = await dbContext.TermMonthPassingMarks
                .AsNoTracking()
                .Where(p =>
                    p.TermMonthID == termMonthId &&
                    p.CurrentClassID == currentClassId &&
                    p.TermID == classTermId)
                .Select(p => (float?)p.PassingMarks)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                CurrentClassID = currentClassId,
                TermMonthID    = termMonthId,
                TermID         = classTermId,
                PassingMarks   = passing ?? 0
            });
        }

        // ── GET api/TeacherPassingMarks/all-for-class ────────────────────────
        // Returns all monthly passing marks + terminal passing mark for a class in one call.
        [HttpGet("all-for-class")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAllForClass([FromQuery] Guid currentClassId)
        {
            if (currentClassId == Guid.Empty) return BadRequest("currentClassId is required.");

            var classTermId = await GetClassTermIdAsync(currentClassId);
            if (classTermId == null) return NotFound("Class not found.");

            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(currentClassId);
                if (!owns) return Forbid();
            }

            // Monthly passing marks
            var monthly = await dbContext.TermMonthPassingMarks
                .AsNoTracking()
                .Where(p => p.CurrentClassID == currentClassId && p.TermID == classTermId)
                .Select(p => new { p.TermMonthID, p.PassingMarks })
                .ToListAsync();

            // Terminal passing mark
            var terminal = await dbContext.TerminalPassingMarks
                .AsNoTracking()
                .Where(p => p.CurrentClassID == currentClassId && p.TermID == classTermId)
                .Select(p => (float?)p.PassingMarks)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                CurrentClassID          = currentClassId,
                TermID                  = classTermId,
                MonthlyPassingMarks     = monthly,
                TerminalPassingMarks    = terminal ?? 0
            });
        }

        // ── POST api/TeacherPassingMarks/upsert-terminal ─────────────────────
        // Set or update the terminal exam passing mark for a class.

        public class UpsertTerminalPassingMarkDto
        {
            public Guid CurrentClassID { get; set; }
            public float PassingMarks { get; set; }
        }

        [HttpPost("upsert-terminal")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpsertTerminal([FromBody] UpsertTerminalPassingMarkDto dto)
        {
            if (dto.CurrentClassID == Guid.Empty) return BadRequest("CurrentClassID is required.");
            if (dto.PassingMarks < 0) return BadRequest("PassingMarks must be >= 0.");

            var classTermId = await GetClassTermIdAsync(dto.CurrentClassID);
            if (classTermId == null) return NotFound("Class not found.");

            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(dto.CurrentClassID);
                if (!owns) return Forbid();
            }

            var existing = await dbContext.TerminalPassingMarks
                .FirstOrDefaultAsync(p =>
                    p.CurrentClassID == dto.CurrentClassID &&
                    p.TermID == classTermId);

            if (existing == null)
            {
                existing = new TerminalPassingMark
                {
                    TerminalPassingMarkID = Guid.NewGuid(),
                    TermID                = classTermId.Value,
                    CurrentClassID        = dto.CurrentClassID,
                    PassingMarks          = dto.PassingMarks,
                    CreatedAt             = DateTime.UtcNow
                };
                dbContext.TerminalPassingMarks.Add(existing);
            }
            else
            {
                existing.PassingMarks = dto.PassingMarks;
                existing.UpdatedAt    = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                existing.TerminalPassingMarkID,
                existing.CurrentClassID,
                existing.TermID,
                existing.PassingMarks
            });
        }

        // ── GET api/TeacherPassingMarks/terminal ─────────────────────────────
        [HttpGet("terminal")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetTerminal([FromQuery] Guid currentClassId)
        {
            if (currentClassId == Guid.Empty) return BadRequest("currentClassId is required.");

            var classTermId = await GetClassTermIdAsync(currentClassId);
            if (classTermId == null) return NotFound("Class not found.");

            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(currentClassId);
                if (!owns) return Forbid();
            }

            var passing = await dbContext.TerminalPassingMarks
                .AsNoTracking()
                .Where(p => p.CurrentClassID == currentClassId && p.TermID == classTermId)
                .Select(p => (float?)p.PassingMarks)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                CurrentClassID = currentClassId,
                TermID         = classTermId,
                PassingMarks   = passing ?? 0
            });
        }
    }
}
