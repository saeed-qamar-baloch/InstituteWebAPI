using AutoMapper;
using InstituteWebAPI.Models.DTO.ClassStudents;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminClassStudentsController : ControllerBase
    {
        private readonly IClassStudentsRepository repository;
        private readonly IMapper mapper;

        public AdminClassStudentsController(IClassStudentsRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var classStudents = await repository.GetAllAsync();
            return Ok(mapper.Map<List<ClassStudentDto>>(classStudents));
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var classStudent = await repository.GetAsync(id);
            if (classStudent == null) return NotFound();
            return Ok(mapper.Map<ClassStudentDto>(classStudent));
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddClassStudentDto dto)
        {
            var classStudent = mapper.Map<ClassStudents>(dto);
            classStudent = await repository.AddAsync(classStudent);
            return Ok(mapper.Map<ClassStudentDto>(classStudent));
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateClassStudentDto dto)
        {
            var updatedEntity = mapper.Map<ClassStudents>(dto);
            var updated = await repository.UpdateAsync(id, updatedEntity);

            if (updated == null) return NotFound();
            return Ok(mapper.Map<ClassStudentDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await repository.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<ClassStudentDto>(deleted));
        }

     
    }
}
