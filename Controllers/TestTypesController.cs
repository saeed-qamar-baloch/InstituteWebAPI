using AutoMapper;
using InstituteWebAPI.Models.DTO.TestType;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class TestTypesController : ControllerBase
    {
        private readonly ITestTypeRepository repository;
        private readonly ITermRepository termRepository;
        private readonly IMapper mapper;

        public TestTypesController(ITestTypeRepository repository, ITermRepository termRepository, IMapper mapper)
        {
            this.repository = repository;
            this.termRepository = termRepository;
            this.mapper = mapper;
        }

        // Resolve the term id when "current term only" is requested.
        private async Task<(bool ok, Guid? termId, string? error)> ResolveTermAsync(bool currentTermOnly)
        {
            if (!currentTermOnly) return (true, null, null);
            var term = await termRepository.GetActiveAsync();
            if (term == null) return (false, null, "No active term to scope this test type to. Mark a term active first.");
            return (true, term.TermID, null);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var list = await repository.GetAllAsync();
            return Ok(mapper.Map<List<TestTypeDto>>(list));
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var item = await repository.GetAsync(id);
            if (item == null) return NotFound();
            return Ok(mapper.Map<TestTypeDto>(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddTestTypeDto dto)
        {
            var (ok, termId, error) = await ResolveTermAsync(dto.CurrentTermOnly);
            if (!ok) return BadRequest(error);

            var entity = mapper.Map<TestType>(dto);
            entity.TermID = termId;
            entity = await repository.AddAsync(entity);
            return Ok(mapper.Map<TestTypeDto>(entity));
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateTestTypeDto dto)
        {
            var (ok, termId, error) = await ResolveTermAsync(dto.CurrentTermOnly);
            if (!ok) return BadRequest(error);

            var entity = mapper.Map<TestType>(dto);
            entity.TermID = termId;
            var updated = await repository.UpdateAsync(id, entity);
            if (updated == null) return NotFound();
            return Ok(mapper.Map<TestTypeDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await repository.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<TestTypeDto>(deleted));
        }
    }
}
