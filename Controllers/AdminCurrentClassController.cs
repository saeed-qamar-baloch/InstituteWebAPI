using AutoMapper;
using InstituteWebAPI.Models.DTO.CurrentClasses;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebAPI.Services.TermContext;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminCurrentClassController : ControllerBase
    {
        private readonly ICurrentClassRepository repository;
        private readonly ITeacherIdentityLinkRepository teacherIdentity;
        private readonly ITermContext termContext;
        private readonly IMapper mapper;

        public AdminCurrentClassController(ICurrentClassRepository repository, ITeacherIdentityLinkRepository teacherIdentity, ITermContext termContext, IMapper mapper)
        {
            this.repository = repository;
            this.teacherIdentity = teacherIdentity;
            this.termContext = termContext;
            this.mapper = mapper;
        }

        private async Task<Guid?> GetTeacherIdFromTokenAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return null;
            return await teacherIdentity.GetTeacherIdForUserIdAsync(userId);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAll()
        {
            var activeTerm = await termContext.GetActiveTermAsync();

            // Teacher can only see their own assigned classes
            if (User.IsInRole("Teacher"))
            {
                var teacherId = await GetTeacherIdFromTokenAsync();
                if (teacherId == null) return Forbid();

                var result = await repository.SearchCurrentClassesAsync(null, null, teacherId.Value, null, activeTerm.TermID, null);
                return Ok(mapper.Map<List<CurrentClassDto>>(result));
            }

            var currentClasses = await repository.SearchCurrentClassesAsync(null, null, null, null, activeTerm.TermID, null);
            return Ok(mapper.Map<List<CurrentClassDto>>(currentClasses));
        }

        [HttpGet("{id:Guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var currentClass = await repository.GetAsync(id);
            if (currentClass == null) return NotFound();

            var activeTerm = await termContext.GetActiveTermAsync();
            if (currentClass.TermID != activeTerm.TermID)
            {
                return Forbid();
            }

            if (User.IsInRole("Teacher"))
            {
                var teacherId = await GetTeacherIdFromTokenAsync();
                if (teacherId == null) return Forbid();

                if (currentClass.TeacherID != teacherId)
                {
                    return Forbid();
                }
            }

            return Ok(mapper.Map<CurrentClassDto>(currentClass));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(AddCurrentClassDto dto)
        {
            var activeTerm = await termContext.GetActiveTermAsync();

            var currentClass = mapper.Map<CurrentClass>(dto);
            currentClass.CreatedOn = DateTime.UtcNow;
            currentClass.IsActive = true;

            // Force active term
            currentClass.TermID = activeTerm.TermID;

            try
            {
                currentClass = await repository.AddAsync(currentClass);
                return Ok(mapper.Map<CurrentClassDto>(currentClass));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateCurrentClassDto dto)
        {
            var existing = await repository.GetAsync(id);
            if (existing == null) return NotFound();

            var activeTerm = await termContext.GetActiveTermAsync();
            if (existing.TermID != activeTerm.TermID)
            {
                return BadRequest("You can only update classes in the active term.");
            }

            var updated = mapper.Map<CurrentClass>(dto);

            // Prevent moving records across terms
            updated.TermID = activeTerm.TermID;

            try
            {
                updated = await repository.UpdateAsync(id, updated);
                if (updated == null) return NotFound();
                return Ok(mapper.Map<CurrentClassDto>(updated));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await repository.GetAsync(id);
            if (existing == null) return NotFound();

            var activeTerm = await termContext.GetActiveTermAsync();
            if (existing.TermID != activeTerm.TermID)
            {
                return BadRequest("You can only delete classes in the active term.");
            }

            var deleted = await repository.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<CurrentClassDto>(deleted));
        }

        [HttpGet("search")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Search([FromQuery] Guid? classID, [FromQuery] Guid? slotID, [FromQuery] Guid? teacherID, [FromQuery] Guid? sessionID, [FromQuery] Guid? termID, [FromQuery] bool? isActive)
        {
            var activeTerm = await termContext.GetActiveTermAsync();

            // Force active term regardless of what client sends
            termID = activeTerm.TermID;

            if (User.IsInRole("Teacher"))
            {
                var teacherIdFromToken = await GetTeacherIdFromTokenAsync();
                if (teacherIdFromToken == null) return Forbid();

                // Force to own teacher id
                teacherID = teacherIdFromToken;
            }

            var result = await repository.SearchCurrentClassesAsync(classID, slotID, teacherID, sessionID, termID, isActive);
            return Ok(mapper.Map<List<CurrentClassDto>>(result));
        }
    }
}
