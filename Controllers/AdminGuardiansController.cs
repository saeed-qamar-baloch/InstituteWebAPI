using AutoMapper;
using InstituteWebAPI.Models.DTO.Guardian;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminGuardiansController : ControllerBase
    {
        private readonly IGuardianRepository _repo;
        private readonly IMapper _mapper;

        public AdminGuardiansController(IGuardianRepository repo, IMapper mapper)
        {
            _repo   = repo;
            _mapper = mapper;
        }

        // GET api/AdminGuardians?studentId=<guid>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? studentId)
        {
            var list = await _repo.GetAllAsync(studentId);
            return Ok(_mapper.Map<List<GuardianDto>>(list));
        }

        // GET api/AdminGuardians/{id}
        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<GuardianDto>(item));
        }

        // GET api/AdminGuardians/by-student/{studentId}
        [HttpGet("by-student/{studentId:Guid}")]
        public async Task<IActionResult> GetByStudent([FromRoute] Guid studentId)
        {
            var list = await _repo.GetByStudentIdAsync(studentId);
            return Ok(_mapper.Map<List<GuardianDto>>(list));
        }

        // POST api/AdminGuardians
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddGuardianDto dto)
        {
            var domain  = _mapper.Map<Guardian>(dto);
            var created = await _repo.AddAsync(domain);
            return CreatedAtAction(nameof(GetById),
                new { id = created.GuardianID },
                _mapper.Map<GuardianDto>(created));
        }

        // PUT api/AdminGuardians/{id}
        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateGuardianDto dto)
        {
            var domain  = _mapper.Map<Guardian>(dto);
            var updated = await _repo.UpdateAsync(id, domain);
            if (updated == null) return NotFound();
            return Ok(_mapper.Map<GuardianDto>(updated));
        }

        // DELETE api/AdminGuardians/{id}
        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(_mapper.Map<GuardianDto>(deleted));
        }
    }
}
