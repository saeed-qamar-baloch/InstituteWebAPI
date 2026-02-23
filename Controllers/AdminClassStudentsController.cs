using AutoMapper;
using InstituteWebAPI.Models.DTO.ClassStudents;
using InstituteWebAPI.Repositories.IRepository;
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
        private readonly ITeacherIdentityLinkRepository teacherIdentity;
        private readonly IMapper mapper;

        public AdminClassStudentsController(IClassStudentsRepository repository, ITeacherIdentityLinkRepository teacherIdentity, IMapper mapper)
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
            var classStudents = await repository.GetAllAsync();

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

                if (classStudent.CurrentClass?.TeacherID != teacherId)
                {
                    return Forbid();
                }
            }

            return Ok(mapper.Map<ClassStudentDto>(classStudent));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(AddClassStudentDto dto)
        {
            var classStudent = mapper.Map<ClassStudents>(dto);
            classStudent = await repository.AddAsync(classStudent);
            return Ok(mapper.Map<ClassStudentDto>(classStudent));
        }

        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateClassStudentDto dto)
        {
            var updatedEntity = mapper.Map<ClassStudents>(dto);
            var updated = await repository.UpdateAsync(id, updatedEntity);

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
