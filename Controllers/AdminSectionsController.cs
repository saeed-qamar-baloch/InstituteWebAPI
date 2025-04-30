using AutoMapper;
using InstituteWebAPI.Models.DTO.Sections;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminSectionsController : ControllerBase
    {
        private readonly ISectionsRepository _sectionsRepo;
        private readonly IMapper _mapper;

        public AdminSectionsController(ISectionsRepository sectionsRepo, IMapper mapper)
        {
            _sectionsRepo = sectionsRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sections = await _sectionsRepo.GetAllAsync();
            return Ok(_mapper.Map<List<SectionsDto>>(sections));
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var section = await _sectionsRepo.GetAsync(id);
            if (section == null) return NotFound();

            return Ok(_mapper.Map<SectionsDto>(section));
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddSectionsDto dto)
        {
            var section = _mapper.Map<Sections>(dto);
            section = await _sectionsRepo.AddAsync(section);

            return Ok(_mapper.Map<SectionsDto>(section));
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, SectionsUpdateDto dto)
        {
            var section = _mapper.Map<Sections>(dto);
            section = await _sectionsRepo.UpdateAsync(id, section);

            if (section == null) return NotFound();

            return Ok(_mapper.Map<SectionsDto>(section));
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _sectionsRepo.DeleteAsync(id);
            if (deleted == null) return NotFound();

            return Ok(_mapper.Map<SectionsDto>(deleted));
        }
    }
}
