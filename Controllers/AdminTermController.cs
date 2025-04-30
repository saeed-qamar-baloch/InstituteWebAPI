using AutoMapper;
using InstituteWebAPI.Models.DTO.Terms;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminTermController : ControllerBase
    {
        private readonly ITermRepository termRepository;
        private readonly IMapper mapper;

        public AdminTermController(ITermRepository termRepository, IMapper mapper)
        {
            this.termRepository = termRepository;
            this.mapper = mapper;
        }
       
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var TermsDomain = await termRepository.GetAllAsync();

            var TermsDto= mapper.Map<List<TermDto>>(TermsDomain);
            return Ok(TermsDto);
        }
        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<ActionResult> GetByID(Guid id)
        {
            var termDomain = await termRepository.GetAsync(id);

            return Ok(mapper.Map<TermDto>(termDomain));
        }
        [HttpGet]
        [Route("{Name}")]
        public async Task<IActionResult> GetByName(string Name)
        {
            var termDomain = await termRepository.GetTermByNameAsync(Name);
            return Ok(mapper.Map<TermDto>(termDomain));
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddTermDto addTermDto)
        {
            
            var termDomainModel = mapper.Map<Term>(addTermDto);
            termDomainModel = await termRepository.AddAsync(termDomainModel);

            var termsDto= mapper.Map<AddTermDto>(termDomainModel);
            return Ok(termsDto);

        }

        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] TermUpdateRequestDto termUpdateRequestDto)
        {
            var termDomainModel = mapper.Map<Term>(termUpdateRequestDto);
            termDomainModel = await termRepository.UpdateAsync(id, termDomainModel);

            if (termDomainModel == null)
                return NotFound();

            return Ok(mapper.Map<TermDto>(termDomainModel));
        }


        [HttpDelete]
        [Route("{id:Guid}")]

        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var deleteTerm = await termRepository.DeleteAsync(id);
            if (deleteTerm == null)
                return NotFound();

            return Ok(mapper.Map<TermDto>(deleteTerm));
        }



    }
}
