using AutoMapper;
using InstituteWebAPI.Models.DTO.TermMonths;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher")]
    public class AdminTermMonthsController : ControllerBase
    {
        private readonly ITermMonthsRepository termMonthsRepository;
        private readonly IMapper mapper;

        public AdminTermMonthsController(ITermMonthsRepository termMonthsRepository, IMapper mapper)
        {
            this.termMonthsRepository = termMonthsRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var termMonthsDomain = await termMonthsRepository.GetAllAsync();
            var termMonthsDto = mapper.Map<List<TermMonthsDto>>(termMonthsDomain);
            return Ok(termMonthsDto);
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetByID(Guid id)
        {
            var termMonthDomain = await termMonthsRepository.GetAsync(id);
            if (termMonthDomain == null)
                return NotFound();

            return Ok(mapper.Map<TermMonthsDto>(termMonthDomain));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] AddTermMonthsDto addTermMonthsDto)
        {
            
            var termMonthsDomainModel = mapper.Map<TermMonths>(addTermMonthsDto);
            termMonthsDomainModel = await termMonthsRepository.AddAsync(termMonthsDomainModel);

            var termMonthsDto = mapper.Map<TermMonthsDto>(termMonthsDomainModel);
            return Ok(termMonthsDto);
        }

        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] TermMonthsUpdateRequestDto termMonthsUpdateRequestDto)
        {
            var termMonthsDomainModel = mapper.Map<TermMonths>(termMonthsUpdateRequestDto);
            termMonthsDomainModel = await termMonthsRepository.UpdateAsync(id, termMonthsDomainModel);

            if (termMonthsDomainModel == null)
                return NotFound();

            return Ok(mapper.Map<TermMonthsDto>(termMonthsDomainModel));
        }

        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var deletedTermMonth = await termMonthsRepository.DeleteAsync(id);
            if (deletedTermMonth == null)
                return NotFound();

            return Ok(mapper.Map<TermMonthsDto>(deletedTermMonth));
        }
    }
}
