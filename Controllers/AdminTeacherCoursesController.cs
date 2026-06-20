using AutoMapper;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.AspNetCore.Mvc;
using InstituteWebAPI.Models.DTO.TeacherCourse;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminTeacherCoursesController : ControllerBase
    {
        private readonly ITeacherCoursesRepository repo;
        private readonly IMapper mapper;

        public AdminTeacherCoursesController(ITeacherCoursesRepository repo, IMapper mapper)
        {
            this.repo = repo;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await repo.GetAllAsync();
            return Ok(mapper.Map<List<TeacherCourseDto>>(data));
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await repo.GetAsync(id);
            if (data == null) return NotFound();
            return Ok(mapper.Map<TeacherCourseDto>(data));
        }

        [HttpGet("by-teacher/{teacherId:Guid}")]
        public async Task<IActionResult> GetByTeacher([FromRoute] Guid teacherId)
        {
            var data = await repo.GetByTeacherAsync(teacherId);
            return Ok(mapper.Map<List<TeacherCourseDto>>(data));
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddTeacherCourseDto dto)
        {
            var domain = mapper.Map<TeacherCourses>(dto);
            domain = await repo.AddAsync(domain);
            return Ok(mapper.Map<TeacherCourseDto>(domain));
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateTeacherCourseDto dto)
        {
            var domain = mapper.Map<TeacherCourses>(dto);
            var updated = await repo.UpdateAsync(id, domain);
            if (updated == null) return NotFound();
            return Ok(mapper.Map<TeacherCourseDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await repo.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<TeacherCourseDto>(deleted));
        }
    }
}
