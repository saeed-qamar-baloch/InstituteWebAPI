using AutoMapper;
using InstituteWebAPI.Models.DTO.Section;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminSectionController : ControllerBase
    {
        private readonly ISectionRepository repo;
        private readonly IMapper mapper;

        public AdminSectionController(ISectionRepository repo, IMapper mapper)
        {
            this.repo = repo;
            this.mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAll()
        {
            var data = await repo.GetAllAsync();
            return Ok(mapper.Map<List<SectionDto>>(data));
        }

        [HttpGet("{id:Guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await repo.GetAsync(id);
            if (data == null) return NotFound();
            return Ok(mapper.Map<SectionDto>(data));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] AddSectionDto dto)
        {
            var domain = mapper.Map<Section>(dto);
            domain = await repo.AddAsync(domain);
            return Ok(mapper.Map<SectionDto>(domain));
        }

        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSectionDto dto)
        {
            var domain = mapper.Map<Section>(dto);
            var updated = await repo.UpdateAsync(id, domain);
            if (updated == null) return NotFound();
            return Ok(mapper.Map<SectionDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await repo.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<SectionDto>(deleted));
        }
    }
}
