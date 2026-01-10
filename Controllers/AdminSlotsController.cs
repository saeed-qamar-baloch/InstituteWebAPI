using AutoMapper;
using InstituteWebAPI.Models.DTO.Slots;
using InstituteWebAPI.Repositories.IRepository;
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
        private readonly IMapper mapper;

        public AdminSlotsController(ISlotsRepository repo, IMapper mapper)
        {
            this.repo = repo;
            this.mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAll()
        {
            var data = await repo.GetAllAsync();
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
            var slot = mapper.Map<Slots>(dto);
            slot = await repo.AddAsync(slot);
            return Ok(mapper.Map<SlotsDto>(slot));
        }

        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, SlotsUpdateDto dto)
        {
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
