using AutoMapper;
using InstituteWebAPI.CustomActionFilters;
using InstituteWebAPI.Models.DTO.Admissions;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminAdmissionsController : ControllerBase
    {
        private readonly IAdmissionsRepository repository;
        private readonly IMapper mapper;

        public AdminAdmissionsController(IAdmissionsRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var admissions = await repository.GetAllAsync();
            return Ok(mapper.Map<List<AdmissionDto>>(admissions));
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var admission = await repository.GetAsync(id);
            if (admission == null) return NotFound();
            return Ok(mapper.Map<AdmissionDto>(admission));
        }


        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Create(AddAdmissionDto dto)
        {
            var admission = mapper.Map<Admissions>(dto);
            admission.CreatedAt = DateTime.UtcNow;
            admission.ModifiedAt = DateTime.UtcNow;

            admission = await repository.AddAsync(admission);
            return Ok(mapper.Map<AdmissionDto>(admission));
        }
        
        [HttpPut("{id:Guid}")]
        [ValidateModel]
        public async Task<IActionResult> Update(Guid id, UpdateAdmissionDto dto)
        {
            var updated = mapper.Map<Admissions>(dto);
            updated = await repository.UpdateAsync(id, updated);

            if (updated == null) return NotFound();
            return Ok(mapper.Map<AdmissionDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await repository.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<AdmissionDto>(deleted));
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? registrationNo, [FromQuery] string? StudentName, [FromQuery] string? fatherName, [FromQuery] string? cnic, [FromQuery] string? fatherContact)
        {
            var result = await repository.SearchAdmissionsAsync(registrationNo, StudentName, fatherName, cnic, fatherContact);
            return Ok(mapper.Map<List<AdmissionDto>>(result));
        }
    }
}
