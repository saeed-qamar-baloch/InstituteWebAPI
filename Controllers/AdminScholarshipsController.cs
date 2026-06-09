using AutoMapper;
using InstituteWebAPI.Models.DTO.Scholarship;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminScholarshipsController : ControllerBase
    {
        private readonly IScholarshipRepository _repo;
        private readonly IMapper _mapper;

        public AdminScholarshipsController(IScholarshipRepository repo, IMapper mapper)
        {
            _repo   = repo;
            _mapper = mapper;
        }

        // GET api/AdminScholarships?studentId=&admissionId=&activeOnly=true
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? studentId,
            [FromQuery] Guid? admissionId,
            [FromQuery] bool activeOnly = false)
        {
            var list = await _repo.GetAllAsync(studentId, admissionId, activeOnly);
            return Ok(_mapper.Map<List<ScholarshipDto>>(list));
        }

        // GET api/AdminScholarships/{id}
        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<ScholarshipDto>(item));
        }

        // GET api/AdminScholarships/by-student/{studentId}
        [HttpGet("by-student/{studentId:Guid}")]
        public async Task<IActionResult> GetByStudent([FromRoute] Guid studentId)
        {
            var list = await _repo.GetByStudentIdAsync(studentId);
            return Ok(_mapper.Map<List<ScholarshipDto>>(list));
        }

        // POST api/AdminScholarships
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddScholarshipDto dto)
        {
            if (dto.ToMonth < dto.FromMonth)
                return BadRequest("ToMonth must be on or after FromMonth.");

            var domain  = _mapper.Map<Scholarship>(dto);
            var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var created = await _repo.AddAsync(domain, userId);

            return CreatedAtAction(nameof(GetById),
                new { id = created.ScholarshipID },
                _mapper.Map<ScholarshipDto>(created));
        }

        // PUT api/AdminScholarships/{id}
        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateScholarshipDto dto)
        {
            if (dto.ToMonth < dto.FromMonth)
                return BadRequest("ToMonth must be on or after FromMonth.");

            var domain  = _mapper.Map<Scholarship>(dto);
            var updated = await _repo.UpdateAsync(id, domain);
            if (updated == null) return NotFound();
            return Ok(_mapper.Map<ScholarshipDto>(updated));
        }

        // DELETE api/AdminScholarships/{id}
        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(_mapper.Map<ScholarshipDto>(deleted));
        }
    }
}
