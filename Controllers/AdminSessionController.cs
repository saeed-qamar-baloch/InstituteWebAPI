using AutoMapper;
using InstituteWebAPI.Models.DTO;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminSessionController : ControllerBase
    {
        private readonly ISessionRepository sessionRepository;
        private readonly IMapper mapper;

        public AdminSessionController(ISessionRepository sessionRepository, IMapper mapper)
        {
            this.sessionRepository = sessionRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sessionsDomain = await sessionRepository.GetAllAsync();
            var sessionsDto = mapper.Map<List<SessionDto>>(sessionsDomain);
            return Ok(sessionsDto);
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetByID(Guid id)
        {
            var sessionDomain = await sessionRepository.GetAsync(id);
            if (sessionDomain == null)
                return NotFound();

            return Ok(mapper.Map<SessionDto>(sessionDomain));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddSessionDto addSessionDto)
        {
            var sessionDomainModel = mapper.Map<Sessions>(addSessionDto);
            sessionDomainModel = await sessionRepository.AddAsync(sessionDomainModel);

            var sessionDto = mapper.Map<SessionDto>(sessionDomainModel);
            return Ok(sessionDto);
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] SessionUpdateRequestDto sessionUpdateRequestDto)
        {
            var sessionDomainModel = mapper.Map<Sessions>(sessionUpdateRequestDto);
            sessionDomainModel = await sessionRepository.UpdateAsync(id, sessionDomainModel);

            if (sessionDomainModel == null)
                return NotFound();

            return Ok(mapper.Map<SessionDto>(sessionDomainModel));
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var deletedSession = await sessionRepository.DeleteAsync(id);
            if (deletedSession == null)
                return NotFound();

            return Ok(mapper.Map<SessionDto>(deletedSession));
        }
    }
}
