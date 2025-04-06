using AutoMapper;
using InstituteWebAPI.Models.DTO;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminSessionsController : ControllerBase
    {
        private readonly ISessionRepository sessionRepository;
        private readonly IMapper mapper;

        public AdminSessionsController(ISessionRepository sessionRepository, IMapper mapper)
        {
            this.sessionRepository = sessionRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sessions = await sessionRepository.GetAllAsync();
            return Ok(mapper.Map<List<SessionsDto>>(sessions));
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var session = await sessionRepository.GetAsync(id);
            if (session == null) return NotFound();

            return Ok(mapper.Map<SessionsDto>(session));
        }

        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var session = await sessionRepository.GetByNameAsync(name);
            if (session == null) return NotFound();

            return Ok(mapper.Map<SessionsDto>(session));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddSessionsDto addDto)
        {
            var domain = mapper.Map<Sessions>(addDto);
            var created = await sessionRepository.AddAsync(domain);
            return Ok(mapper.Map<SessionsDto>(created));
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SessionUpdateDto updateDto)
        {
            var domain = mapper.Map<Sessions>(updateDto);
            var updated = await sessionRepository.UpdateAsync(id, domain);
            if (updated == null) return NotFound();

            return Ok(mapper.Map<SessionsDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await sessionRepository.DeleteAsync(id);
            if (deleted == null) return NotFound();

            return Ok(mapper.Map<SessionsDto>(deleted));
        }
    }
}
