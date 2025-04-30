using AutoMapper;
using InstituteWebAPI.Models.DTO.Tests;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherTestsController : ControllerBase
    {
        private readonly ITestsRepository repository;
        private readonly IMapper mapper;

        public TeacherTestsController(ITestsRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tests = await repository.GetAllAsync();
            return Ok(mapper.Map<List<TestDto>>(tests));
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var test = await repository.GetAsync(id);
            if (test == null) return NotFound();
            return Ok(mapper.Map<TestDto>(test));
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddTestDto dto)
        {
            var test = mapper.Map<Tests>(dto);
       

            test = await repository.AddAsync(test);
            return Ok(mapper.Map<TestDto>(test));
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateTestDto dto)
        {
            var updated = mapper.Map<Tests>(dto);
            updated = await repository.UpdateAsync(id, updated);

            if (updated == null) return NotFound();
            return Ok(mapper.Map<TestDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await repository.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<TestDto>(deleted));
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? testType, [FromQuery] Guid? termMonthID, [FromQuery] Guid? currentClassID)
        {
            var result = await repository.SearchTestsAsync(testType, termMonthID, currentClassID);
            return Ok(mapper.Map<List<TestDto>>(result));
        }
    }
}
