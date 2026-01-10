using AutoMapper;
using InstituteWebAPI.Models.DTO.CurrentClasses;
using InstituteWebAPI.Repositories.IRepository;
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
        private readonly IMapper mapper;

        public AdminCurrentClassController(ICurrentClassRepository repository, ITeacherIdentityLinkRepository teacherIdentity, IMapper mapper)
        {
            this.repository = repository;
            this.teacherIdentity = teacherIdentity;
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
            // Teacher can only see their own assigned classes
            if (User.IsInRole("Teacher"))
            {
                var teacherId = await GetTeacherIdFromTokenAsync();
                if (teacherId == null) return Forbid();

                var result = await repository.SearchCurrentClassesAsync(null, null, teacherId.Value, null, null, null);
                return Ok(mapper.Map<List<CurrentClassDto>>(result));
            }

            var currentClasses = await repository.GetAllAsync();
            return Ok(mapper.Map<List<CurrentClassDto>>(currentClasses));
        }

        [HttpGet("{id:Guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var currentClass = await repository.GetAsync(id);
            if (currentClass == null) return NotFound();

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
            var currentClass = mapper.Map<CurrentClass>(dto);
            currentClass.CreatedOn = DateTime.UtcNow;
            currentClass.IsActive = true;

            currentClass = await repository.AddAsync(currentClass);
            return Ok(mapper.Map<CurrentClassDto>(currentClass));
        }

        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateCurrentClassDto dto)
        {
            var updated = mapper.Map<CurrentClass>(dto);
            updated = await repository.UpdateAsync(id, updated);

            if (updated == null) return NotFound();
            return Ok(mapper.Map<CurrentClassDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await repository.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<CurrentClassDto>(deleted));
        }

        [HttpGet("search")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Search([FromQuery] Guid? classID, [FromQuery] Guid? slotID, [FromQuery] Guid? teacherID, [FromQuery] Guid? sessionID, [FromQuery] Guid? termID, [FromQuery] bool? isActive)
        {
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
