using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using InstituteWebAPI.Models.DTO.FeeType;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminFeeTypesController : ControllerBase
    {
        private readonly IFeeTypeRepository repository;
        private readonly IMapper mapper;

        public AdminFeeTypesController(IFeeTypeRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await repository.GetAllAsync();
            return Ok(mapper.Map<List<FeeTypeDto>>(list));
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var item = await repository.GetAsync(id);
            if (item == null) return NotFound();
            return Ok(mapper.Map<FeeTypeDto>(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddFeeTypeDto dto)
        {
            var entity = mapper.Map<FeeType>(dto);
            entity = await repository.AddAsync(entity);
            return Ok(mapper.Map<FeeTypeDto>(entity));
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateFeeTypeDto dto)
        {
            var entity = mapper.Map<FeeType>(dto);
            var updated = await repository.UpdateAsync(id, entity);
            if (updated == null) return NotFound();
            return Ok(mapper.Map<FeeTypeDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await repository.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<FeeTypeDto>(deleted));
        }
    }
}
