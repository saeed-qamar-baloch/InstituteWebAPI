using AutoMapper;
using InstituteWebAPI.Models.DTO.ClassStudents;
using InstituteWebAPI.Models.DTO.Students;
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
    public class AdminClassStudentsController : ControllerBase
    {
        private readonly IClassStudentsRepository repository;
        private readonly ICurrentClassRepository currentClassRepository;
        private readonly ITeacherIdentityLinkRepository teacherIdentity;
        private readonly ITermContext termContext;
        private readonly IMapper mapper;

        public AdminClassStudentsController(
            IClassStudentsRepository repository,
            ICurrentClassRepository currentClassRepository,
            ITeacherIdentityLinkRepository teacherIdentity,
            ITermContext termContext,
            IMapper mapper)
        {
            this.repository              = repository;
            this.currentClassRepository  = currentClassRepository;
            this.teacherIdentity         = teacherIdentity;
            this.termContext             = termContext;
            this.mapper                  = mapper;
        }

        /// <summary>
        /// Ensures the target class exists and belongs to the active term.
        /// Returns the active term ID on success, or an error result.
        /// </summary>
        private async Task<(bool ok, IActionResult? error)> ValidateClassInActiveTermAsync(Guid currentClassId)
        {
            var activeTerm   = await termContext.GetActiveTermAsync();
            var currentClass = await currentClassRepository.GetAsync(currentClassId);

            if (currentClass == null)
                return (false, BadRequest(new { message = "Selected class was not found." }));

            if (currentClass.TermID != activeTerm.TermID)
                return (false, BadRequest(new { message = "Students can only be assigned to classes in the active term." }));

            return (true, null);
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
            var activeTerm    = await termContext.GetActiveTermAsync();
            var classStudents = await repository.GetByTermAsync(activeTerm.TermID);

            if (User.IsInRole("Teacher"))
            {
                var teacherId = await GetTeacherIdFromTokenAsync();
                if (teacherId == null) return Forbid();
                classStudents = classStudents
                    .Where(cs => cs.CurrentClass?.TeacherID == teacherId)
                    .ToList();
            }

            return Ok(mapper.Map<List<ClassStudentDto>>(classStudents));
        }

        [HttpGet("{id:Guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var classStudent = await repository.GetAsync(id);
            if (classStudent == null) return NotFound();

            if (User.IsInRole("Teacher"))
            {
                var teacherId = await GetTeacherIdFromTokenAsync();
                if (teacherId == null) return Forbid();
                if (classStudent.CurrentClass?.TeacherID != teacherId) return Forbid();
            }

            return Ok(mapper.Map<ClassStudentDto>(classStudent));
        }

        [HttpGet("by-class/{currentClassId:Guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetByClass([FromRoute] Guid currentClassId)
        {
            var classStudents = await repository.GetByClassAsync(currentClassId);
            return Ok(mapper.Map<List<ClassStudentDto>>(classStudents));
        }

        [HttpGet("unenrolled")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUnenrolled()
        {
            var activeTerm = await termContext.GetActiveTermAsync();
            var students   = await repository.GetUnenrolledStudentsAsync(activeTerm.TermID);
            return Ok(mapper.Map<List<StudentDto>>(students));
        }

        // ── Single assign ──────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(AddClassStudentDto dto)
        {
            var (ok, error) = await ValidateClassInActiveTermAsync(dto.CurrentClassID);
            if (!ok) return error!;

            try
            {
                var classStudent = mapper.Map<ClassStudents>(dto);
                classStudent = await repository.AddAsync(classStudent);
                return Ok(mapper.Map<ClassStudentDto>(classStudent));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── Bulk assign ────────────────────────────────────────────
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkCreate([FromBody] BulkAddClassStudentDto dto)
        {
            if (!dto.StudentIDs.Any())
                return BadRequest(new { message = "No students provided." });

            var (ok, error) = await ValidateClassInActiveTermAsync(dto.CurrentClassID);
            if (!ok) return error!;

            var (assigned, skippedNames) = await repository.BulkAddAsync(
                dto.CurrentClassID, dto.StudentIDs, dto.Status ?? "Enrolled");

            return Ok(new BulkAddResultDto
            {
                Assigned     = assigned,
                Skipped      = skippedNames.Count,
                SkippedNames = skippedNames,
            });
        }

        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateClassStudentDto dto)
        {
            var (ok, error) = await ValidateClassInActiveTermAsync(dto.CurrentClassID);
            if (!ok) return error!;

            var updatedEntity = mapper.Map<ClassStudents>(dto);
            var updated       = await repository.UpdateAsync(id, updatedEntity);
            if (updated == null) return NotFound();
            return Ok(mapper.Map<ClassStudentDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await repository.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<ClassStudentDto>(deleted));
        }
    }
}
