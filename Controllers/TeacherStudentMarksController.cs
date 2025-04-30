using AutoMapper;
using InstituteWebAPI.Models.DTO.StudentMarks;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherStudentMarksController : ControllerBase
    {
        private readonly IStudentMarksRepository repository;
        private readonly IMapper mapper;

        public TeacherStudentMarksController(IStudentMarksRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var studentMarks = await repository.GetAllAsync();
            return Ok(mapper.Map<List<StudentMarksDto>>(studentMarks));
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var studentMarks = await repository.GetAsync(id);
            if (studentMarks == null) return NotFound();
            return Ok(mapper.Map<StudentMarksDto>(studentMarks));
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddStudentMarksDto dto)
        {
            var studentMarks = mapper.Map<StudentMarks>(dto);
            studentMarks = await repository.AddAsync(studentMarks);
            return Ok(mapper.Map<StudentMarksDto>(studentMarks));
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateStudentMarksDto dto)
        {
            var updated = mapper.Map<StudentMarks>(dto);
            updated = await repository.UpdateAsync(id, updated);

            if (updated == null) return NotFound();
            return Ok(mapper.Map<StudentMarksDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await repository.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<StudentMarksDto>(deleted));
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] Guid? studentId, [FromQuery] Guid? testId)
        {
            List<StudentMarks> result = null;

            if (studentId.HasValue)
                result = await repository.GetByStudentIdAsync(studentId.Value);
            else if (testId.HasValue)
                result = await repository.GetByTestIdAsync(testId.Value);

            if (result == null || result.Count == 0) return NotFound();

            return Ok(mapper.Map<List<StudentMarksDto>>(result));
        }
    }
}
