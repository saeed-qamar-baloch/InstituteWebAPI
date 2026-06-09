using AutoMapper;
using InstituteWebAPI.Models.DTO.ResultApproval;
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
    public class AdminResultApprovalsController : ControllerBase
    {
        private readonly IResultApprovalRepository _repo;
        private readonly IMapper _mapper;

        public AdminResultApprovalsController(IResultApprovalRepository repo, IMapper mapper)
        {
            _repo   = repo;
            _mapper = mapper;
        }

        // GET api/AdminResultApprovals?termId=&currentClassId=
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? termId,
            [FromQuery] Guid? currentClassId)
        {
            var list = await _repo.GetAllAsync(termId, currentClassId);
            return Ok(_mapper.Map<List<ResultApprovalDto>>(list));
        }

        // GET api/AdminResultApprovals/{id}
        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<ResultApprovalDto>(item));
        }

        // GET api/AdminResultApprovals/status?termId=&currentClassId=
        // Lightweight check — returns { isApproved: true/false }
        [HttpGet("status")]
        [Authorize(Roles = "Admin,Teacher")]   // teachers also need to check lock status
        public async Task<IActionResult> GetStatus(
            [FromQuery] Guid termId,
            [FromQuery] Guid currentClassId)
        {
            if (termId == Guid.Empty || currentClassId == Guid.Empty)
                return BadRequest("termId and currentClassId are required.");

            var record     = await _repo.GetByClassAndTermAsync(termId, currentClassId);
            var isApproved = record?.IsApproved ?? false;

            return Ok(new
            {
                isApproved,
                approvedAt       = record?.ApprovedAt,
                approvedByUserID = record?.ApprovedByUserID,
                remarks          = record?.Remarks
            });
        }

        // POST api/AdminResultApprovals/upsert
        // Creates or toggles (approve / un-approve) a result lock for a class/term.
        [HttpPost("upsert")]
        public async Task<IActionResult> Upsert([FromBody] UpsertResultApprovalDto dto)
        {
            var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var domain  = _mapper.Map<ResultApproval>(dto);
            var result  = await _repo.UpsertAsync(domain, userId);
            return Ok(_mapper.Map<ResultApprovalDto>(result));
        }
    }
}
