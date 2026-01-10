using AutoMapper;
using InstituteWebAPI.Data;
using InstituteWebAPI.Models.DTO.Tests;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherTestsController : ControllerBase
    {
        private readonly ITestsRepository repository;
        private readonly ICurrentClassRepository currentClassRepository;
        private readonly ITeacherIdentityLinkRepository teacherIdentity;
        private readonly RozhnInstituteDbContext dbContext;
        private readonly IMapper mapper;

        public TeacherTestsController(
            ITestsRepository repository,
            ICurrentClassRepository currentClassRepository,
            ITeacherIdentityLinkRepository teacherIdentity,
            RozhnInstituteDbContext dbContext,
            IMapper mapper)
        {
            this.repository = repository;
            this.currentClassRepository = currentClassRepository;
            this.teacherIdentity = teacherIdentity;
            this.dbContext = dbContext;
            this.mapper = mapper;
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

        private async Task<List<Guid>> GetTeacherCurrentClassIdsAsync()
        {
            var teacherId = await GetTeacherIdFromTokenAsync();
            if (teacherId == null) return new List<Guid>();

            var all = await currentClassRepository.GetAllAsync();
            return all.Where(c => c.TeacherID == teacherId).Select(c => c.CurrentClassID).ToList();
        }

        private async Task<bool> DuplicateExistsAsync(Guid termMonthId, Guid currentClassId, string testType, Guid? excludeTestId = null)
        {
            var q = dbContext.Tests.AsNoTracking().Where(t =>
                t.TermMonthID == termMonthId &&
                t.CurrentClassID == currentClassId &&
                t.TestType == testType);

            if (excludeTestId.HasValue)
            {
                q = q.Where(t => t.TestID != excludeTestId.Value);
            }

            return await q.AnyAsync();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAll()
        {
            if (User.IsInRole("Teacher"))
            {
                var allowedClassIds = await GetTeacherCurrentClassIdsAsync();
                var tests = await dbContext.Tests
                    .Include(t => t.TermMonth)
                    .Include(t => t.CurrentClass)
                    .Where(t => t.CurrentClassID != null && allowedClassIds.Contains(t.CurrentClassID))
                    .ToListAsync();

                return Ok(mapper.Map<List<TestDto>>(tests));
            }

            var allTests = await repository.GetAllAsync();
            return Ok(mapper.Map<List<TestDto>>(allTests));
        }

        [HttpGet("{id:Guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var test = await repository.GetAsync(id);
            if (test == null) return NotFound();

            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(test.CurrentClassID);
                if (!owns) return Forbid();
            }

            return Ok(mapper.Map<TestDto>(test));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Create(AddTestDto dto)
        {
            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(dto.CurrentClassID);
                if (!owns) return Forbid();
            }

            var dup = await DuplicateExistsAsync(dto.TermMonthID, dto.CurrentClassID, dto.TestType);
            if (dup)
            {
                return BadRequest("Duplicate test is not allowed for the same month, class and test type.");
            }

            var test = mapper.Map<Tests>(dto);
            test = await repository.AddAsync(test);
            return Ok(mapper.Map<TestDto>(test));
        }

        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Update(Guid id, UpdateTestDto dto)
        {
            if (User.IsInRole("Teacher"))
            {
                var existing = await repository.GetAsync(id);
                if (existing == null) return NotFound();

                var ownsExisting = await TeacherOwnsCurrentClass(existing.CurrentClassID);
                if (!ownsExisting) return Forbid();

                var ownsNew = await TeacherOwnsCurrentClass(dto.CurrentClassID);
                if (!ownsNew) return Forbid();
            }

            var dup = await DuplicateExistsAsync(dto.TermMonthID, dto.CurrentClassID, dto.TestType, excludeTestId: id);
            if (dup)
            {
                return BadRequest("Duplicate test is not allowed for the same month, class and test type.");
            }

            var updated = mapper.Map<Tests>(dto);
            updated = await repository.UpdateAsync(id, updated);

            if (updated == null) return NotFound();
            return Ok(mapper.Map<TestDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (User.IsInRole("Teacher"))
            {
                var existing = await repository.GetAsync(id);
                if (existing == null) return NotFound();

                var owns = await TeacherOwnsCurrentClass(existing.CurrentClassID);
                if (!owns) return Forbid();
            }

            var deleted = await repository.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<TestDto>(deleted));
        }

        [HttpGet("search")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Search([FromQuery] string? testType, [FromQuery] Guid? termMonthID, [FromQuery] Guid? currentClassID)
        {
            if (User.IsInRole("Teacher"))
            {
                var allowedClassIds = await GetTeacherCurrentClassIdsAsync();
                if (!allowedClassIds.Any())
                {
                    return Ok(new List<TestDto>());
                }

                var query = dbContext.Tests
                    .Include(t => t.TermMonth)
                    .Include(t => t.CurrentClass)
                    .Where(t => allowedClassIds.Contains(t.CurrentClassID));

                if (!string.IsNullOrWhiteSpace(testType))
                {
                    query = query.Where(t => t.TestType.Contains(testType));
                }

                if (termMonthID.HasValue)
                {
                    query = query.Where(t => t.TermMonthID == termMonthID);
                }

                if (currentClassID.HasValue)
                {
                    if (!allowedClassIds.Contains(currentClassID.Value)) return Forbid();
                    query = query.Where(t => t.CurrentClassID == currentClassID);
                }

                var result = await query.ToListAsync();
                return Ok(mapper.Map<List<TestDto>>(result));
            }

            var adminResult = await repository.SearchTestsAsync(testType, termMonthID, currentClassID);
            return Ok(mapper.Map<List<TestDto>>(adminResult));
        }
    }
}
