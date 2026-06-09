using AutoMapper;
using InstituteWebAPI.Models.DTO.MarkEditRequest;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher")]
    public class MarkEditRequestsController : ControllerBase
    {
        private readonly IMarkEditRequestRepository _repo;
        private readonly ITeacherIdentityLinkRepository _teacherIdentity;
        private readonly IMapper _mapper;
        private readonly InstituteWebAPI.Services.Notifications.IAppNotificationService _notifications;

        public MarkEditRequestsController(
            IMarkEditRequestRepository repo,
            ITeacherIdentityLinkRepository teacherIdentity,
            IMapper mapper,
            InstituteWebAPI.Services.Notifications.IAppNotificationService notifications)
        {
            _repo            = repo;
            _teacherIdentity = teacherIdentity;
            _mapper          = mapper;
            _notifications   = notifications;
        }

        // ── Helper ───────────────────────────────────────────────────────────
        private async Task<Guid?> GetTeacherIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return null;
            return await _teacherIdentity.GetTeacherIdForUserIdAsync(userId);
        }

        // ── GET api/MarkEditRequests ─────────────────────────────────────────
        // Admin: sees all; Teacher: sees only their own.
        // Optional filter: ?status=1 (Pending) | 2 (Approved) | 3 (Rejected)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? status)
        {
            Guid? teacherFilter = null;

            if (User.IsInRole("Teacher"))
            {
                teacherFilter = await GetTeacherIdAsync();
                if (teacherFilter == null) return Forbid();
            }

            var list = await _repo.GetAllAsync(teacherFilter, status);
            return Ok(_mapper.Map<List<MarkEditRequestDto>>(list));
        }

        // ── GET api/MarkEditRequests/{id} ────────────────────────────────────
        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();

            // Teachers may only view their own requests
            if (User.IsInRole("Teacher"))
            {
                var teacherId = await GetTeacherIdAsync();
                if (item.TeacherID != teacherId) return Forbid();
            }

            return Ok(_mapper.Map<MarkEditRequestDto>(item));
        }

        // ── POST api/MarkEditRequests ────────────────────────────────────────
        // Teachers submit a new request; TeacherID resolved from JWT.
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Add([FromBody] AddMarkEditRequestDto dto)
        {
            var teacherId = await GetTeacherIdAsync();
            if (teacherId == null)
                return BadRequest("Teacher profile not linked to this account.");

            var domain = _mapper.Map<MarkEditRequest>(dto);
            domain.TeacherID = teacherId.Value;

            MarkEditRequest created;
            try
            {
                created = await _repo.AddAsync(domain);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }

            var createdDto = _mapper.Map<MarkEditRequestDto>(created);

            await _notifications.NotifyRoleAsync(
                "Admin",
                AppNotificationType.MarkEditRequest,
                "New mark edit request",
                $"{createdDto.TeacherName ?? "A teacher"} requested to edit marks for {createdDto.StudentName ?? "a student"}.",
                "/marks");

            return CreatedAtAction(nameof(GetById),
                new { id = created.RequestID },
                createdDto);
        }

        // ── POST api/MarkEditRequests/{id}/review ────────────────────────────
        // Admin approves or rejects a pending request.
        [HttpPost("{id:Guid}/review")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Review([FromRoute] Guid id, [FromBody] ReviewMarkEditRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            MarkEditRequest? result;
            try
            {
                result = await _repo.ReviewAsync(
                    id,
                    (MarkEditRequestStatus)dto.Status,
                    userId,
                    dto.ReviewRemarks);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            if (result == null) return NotFound();
            return Ok(_mapper.Map<MarkEditRequestDto>(result));
        }
    }
}
