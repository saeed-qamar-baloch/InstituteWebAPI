using InstituteWebAPI.Data;
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
    public class TeacherPassingMarksController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly Repositories.IRepository.ICurrentClassRepository currentClassRepository;
        private readonly Repositories.IRepository.ITeacherIdentityLinkRepository teacherIdentity;
        private readonly ITermContext termContext;

        public TeacherPassingMarksController(
            RozhnInstituteDbContext dbContext,
            Repositories.IRepository.ICurrentClassRepository currentClassRepository,
            Repositories.IRepository.ITeacherIdentityLinkRepository teacherIdentity,
            ITermContext termContext)
        {
            this.dbContext = dbContext;
            this.currentClassRepository = currentClassRepository;
            this.teacherIdentity = teacherIdentity;
            this.termContext = termContext;
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

        private async Task<bool> CurrentClassIsInActiveTermAsync(Guid currentClassId)
        {
            var active = await termContext.GetActiveTermAsync();
            var cc = await currentClassRepository.GetAsync(currentClassId);
            return cc != null && cc.TermID == active.TermID;
        }

        public class UpsertPassingMarksDto
        {
            public Guid CurrentClassID { get; set; }
            public Guid TermMonthID { get; set; }
            public float PassingMarks { get; set; }
        }

        // Upsert passing marks for a month for one of the teacher's classes (scoped to active term + class)
        [HttpPost("upsert")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Upsert([FromBody] UpsertPassingMarksDto dto)
        {
            if (dto.CurrentClassID == Guid.Empty) return BadRequest("CurrentClassID is required.");
            if (dto.TermMonthID == Guid.Empty) return BadRequest("TermMonthID is required.");
            if (dto.PassingMarks < 0) return BadRequest("PassingMarks must be >= 0.");

            if (!await CurrentClassIsInActiveTermAsync(dto.CurrentClassID))
            {
                return BadRequest("Passing marks can only be managed for classes in the active term.");
            }

            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(dto.CurrentClassID);
                if (!owns) return Forbid();
            }

            // Validate month exists
            var monthExists = await dbContext.TermMonths.AsNoTracking().AnyAsync(m => m.TermMonthID == dto.TermMonthID);
            if (!monthExists) return NotFound("Term month not found.");

            var activeTerm = await termContext.GetActiveTermAsync();

            var existing = await dbContext.TermMonthPassingMarks
                .FirstOrDefaultAsync(p =>
                    p.CurrentClassID == dto.CurrentClassID &&
                    p.TermID == activeTerm.TermID &&
                    p.TermMonthID == dto.TermMonthID);

            if (existing == null)
            {
                existing = new TermMonthPassingMark
                {
                    TermMonthPassingMarkID = Guid.NewGuid(),
                    TermID = activeTerm.TermID,
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

        // Get current passing marks for the selected month (scoped to active term + class)
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Get([FromQuery] Guid currentClassId, [FromQuery] Guid termMonthId)
        {
            if (currentClassId == Guid.Empty) return BadRequest("currentClassId is required.");
            if (termMonthId == Guid.Empty) return BadRequest("termMonthId is required.");

            if (!await CurrentClassIsInActiveTermAsync(currentClassId))
            {
                return BadRequest("Passing marks can only be viewed for classes in the active term.");
            }

            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(currentClassId);
                if (!owns) return Forbid();
            }

            var activeTerm = await termContext.GetActiveTermAsync();

            var passing = await dbContext.TermMonthPassingMarks
                .AsNoTracking()
                .Where(p =>
                    p.TermMonthID == termMonthId &&
                    p.CurrentClassID == currentClassId &&
                    p.TermID == activeTerm.TermID)
                .Select(p => (float?)p.PassingMarks)
                .FirstOrDefaultAsync();

            return Ok(new { CurrentClassID = currentClassId, TermMonthID = termMonthId, TermID = activeTerm.TermID, PassingMarks = passing ?? 0 });
        }
    }
}
