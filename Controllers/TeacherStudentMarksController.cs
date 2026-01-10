using AutoMapper;
using InstituteWebAPI.Models.DTO.StudentMarks;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IMapper mapper;

        public TeacherStudentMarksController(
            IStudentMarksRepository repository,
            ITestsRepository testsRepository,
            ICurrentClassRepository currentClassRepository,
            ITeacherIdentityLinkRepository teacherIdentity,
            IMapper mapper)
        {
            this.repository = repository;
            this.testsRepository = testsRepository;
            this.currentClassRepository = currentClassRepository;
            this.teacherIdentity = teacherIdentity;
            this.mapper = mapper;
        }

        private async Task<Guid?> GetTeacherIdFromTokenAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return null;
            return await teacherIdentity.GetTeacherIdForUserIdAsync(userId);
        }

        private async Task<bool> TeacherOwnsTest(Guid testId)
        {
            var test = await testsRepository.GetAsync(testId);
            if (test == null) return false;

            if (test.CurrentClassID == null) return false;

            var currentClass = await currentClassRepository.GetAsync(test.CurrentClassID);
            if (currentClass == null) return false;

            var teacherIdFromToken = await GetTeacherIdFromTokenAsync();
            if (teacherIdFromToken == null) return false;

            return currentClass.TeacherID == teacherIdFromToken;
        }

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
                // Require testId so we can enforce ownership
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
    }
}
