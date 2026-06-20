using AutoMapper;
using InstituteWebAPI.Models.DTO.Villages;
using Microsoft.AspNetCore.Authorization;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminVillageController : ControllerBase
    {
        private readonly IVillageRepository villageRepository;
        private readonly IMapper mapper;

        public AdminVillageController(IVillageRepository villageRepository, IMapper mapper)
        {
            this.villageRepository = villageRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var villagesDomain = await villageRepository.GetAllAsync();
            var villagesDto = mapper.Map<List<VillageDto>>(villagesDomain);
            return Ok(villagesDto);
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetByID(Guid id)
        {
            var villageDomain = await villageRepository.GetAsync(id);
            if (villageDomain == null)
                return NotFound();

            return Ok(mapper.Map<VillageDto>(villageDomain));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddVillageDto addVillageDto)
        {
            var villageDomainModel = mapper.Map<Village>(addVillageDto);
            villageDomainModel = await villageRepository.AddAsync(villageDomainModel);

            var villageDto = mapper.Map<VillageDto>(villageDomainModel);
            return Ok(villageDto);
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] VillageUpdateRequestDto villageUpdateRequestDto)
        {
            var villageDomainModel = mapper.Map<Village>(villageUpdateRequestDto);
            villageDomainModel = await villageRepository.UpdateAsync(id, villageDomainModel);

            if (villageDomainModel == null)
                return NotFound();

            return Ok(mapper.Map<VillageDto>(villageDomainModel));
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var deletedVillage = await villageRepository.DeleteAsync(id);
            if (deletedVillage == null)
                return NotFound();

            return Ok(mapper.Map<VillageDto>(deletedVillage));
        }
    }
}
