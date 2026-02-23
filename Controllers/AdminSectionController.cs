using AutoMapper;
using InstituteWebAPI.Models.DTO.Section;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebAPI.Services.TermContext;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminSectionController : ControllerBase
    {
        private readonly ISectionRepository repo;
        private readonly ITermContext termContext;
        private readonly IMapper mapper;

        public AdminSectionController(ISectionRepository repo, ITermContext termContext, IMapper mapper)
        {
            this.repo = repo;
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
            var activeTerm = await termContext.GetActiveTermAsync();
            dto.TermID = activeTerm.TermID;
            var domain = mapper.Map<Section>(dto);
            domain = await repo.AddAsync(domain);
            return Ok(mapper.Map<SectionDto>(domain));
        }

        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSectionDto dto)
        {
            var activeTerm = await termContext.GetActiveTermAsync();
            dto.TermID = activeTerm.TermID;
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
