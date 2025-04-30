using AutoMapper;
using InstituteWebAPI.Models.DTO.CurrentClasses;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminCurrentClassController : ControllerBase
    {
        private readonly ICurrentClassRepository repository;
        private readonly IMapper mapper;

        public AdminCurrentClassController(ICurrentClassRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var currentClasses = await repository.GetAllAsync();
            return Ok(mapper.Map<List<CurrentClassDto>>(currentClasses));
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var currentClass = await repository.GetAsync(id);
            if (currentClass == null) return NotFound();
            return Ok(mapper.Map<CurrentClassDto>(currentClass));
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddCurrentClassDto dto)
        {
            var currentClass = mapper.Map<CurrentClass>(dto);
            currentClass.CreatedOn = DateTime.UtcNow;
            currentClass.IsActive = true;

            currentClass = await repository.AddAsync(currentClass);
            return Ok(mapper.Map<CurrentClassDto>(currentClass));
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateCurrentClassDto dto)
        {
            var updated = mapper.Map<CurrentClass>(dto);
            updated = await repository.UpdateAsync(id, updated);

            if (updated == null) return NotFound();
            return Ok(mapper.Map<CurrentClassDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await repository.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<CurrentClassDto>(deleted));
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] Guid? classID, [FromQuery] Guid? sectionID, [FromQuery] Guid? teacherID, [FromQuery] Guid? sessionID, [FromQuery] Guid? termID, [FromQuery] bool? isActive)
        {
            var result = await repository.SearchCurrentClassesAsync(classID, sectionID, teacherID, sessionID, termID, isActive);
            return Ok(mapper.Map<List<CurrentClassDto>>(result));
        }
    }
}
