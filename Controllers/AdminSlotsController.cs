using AutoMapper;
using InstituteWebAPI.Models.DTO.Slots;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebAPI.Services.TermContext;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminSlotsController : ControllerBase
    {
        private readonly ISlotsRepository repo;
        private readonly ICoursesRepository coursesRepository;
        private readonly ITermContext termContext;
        private readonly IMapper mapper;

        public AdminSlotsController(ISlotsRepository repo, ICoursesRepository coursesRepository, ITermContext termContext, IMapper mapper)
        {
            this.repo = repo;
            this.coursesRepository = coursesRepository;
            this.termContext = termContext;
            this.mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAll()
        {
            var activeTerm = await termContext.GetActiveTermAsync();
            var data = (await repo.GetAllAsync())
                .Where(s => s.TermID == activeTerm.TermID)
                .ToList();
            return Ok(mapper.Map<List<SlotsDto>>(data));
        }

        [HttpGet("{id:Guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var slot = await repo.GetAsync(id);
            if (slot == null) return NotFound();
            return Ok(mapper.Map<SlotsDto>(slot));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(AddSlotsDto dto)
        {
            var activeTerm = await termContext.GetActiveTermAsync();
            dto.TermID = activeTerm.TermID;

            var course = await coursesRepository.GetAsnyc(dto.CourseID);
            if (course == null)
            {
                return BadRequest("Course not found.");
            }

            var slot = mapper.Map<Slots>(dto);
            slot = await repo.AddAsync(slot);
            return Ok(mapper.Map<SlotsDto>(slot));
        }

        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, SlotsUpdateDto dto)
        {
            var activeTerm = await termContext.GetActiveTermAsync();
            dto.TermID = activeTerm.TermID;

            var course = await coursesRepository.GetAsnyc(dto.CourseID);
            if (course == null)
            {
                return BadRequest("Course not found.");
            }

            var slot = mapper.Map<Slots>(dto);
            var updated = await repo.UpdateAsync(id, slot);
            if (updated == null) return NotFound();
            return Ok(mapper.Map<SlotsDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await repo.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<SlotsDto>(deleted));
        }
    }
}
