using AutoMapper;
using InstituteWebAPI.Models.DTO.TeacherDailyAttendance;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherDailyAttendanceController : ControllerBase
    {
        private readonly ITeacherDailyAttendanceRepository _repo;
        private readonly IMapper _mapper;

        public TeacherDailyAttendanceController(
            ITeacherDailyAttendanceRepository repo,
            IMapper mapper)
        {
            _repo   = repo;
            _mapper = mapper;
        }

        // ── GET api/TeacherDailyAttendance ───────────────────────────────────
        // ?teacherId=  &fromDate=2026-05-01  &toDate=2026-05-31
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? teacherId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var list = await _repo.GetAllAsync(teacherId, fromDate, toDate);
            return Ok(_mapper.Map<List<TeacherDailyAttendanceDto>>(list));
        }

        // ── GET api/TeacherDailyAttendance/{id} ──────────────────────────────
        [HttpGet("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<TeacherDailyAttendanceDto>(item));
        }

        // ── GET api/TeacherDailyAttendance/by-teacher-date ───────────────────
        // ?teacherId=  &date=2026-05-23
        [HttpGet("by-teacher-date")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByTeacherAndDate(
            [FromQuery] Guid teacherId,
            [FromQuery] DateTime date)
        {
            var item = await _repo.GetByTeacherAndDateAsync(teacherId, date);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<TeacherDailyAttendanceDto>(item));
        }

        // ── POST api/TeacherDailyAttendance/mark ─────────────────────────────
        // Admin manually marks or corrects attendance (upsert).
        [HttpPost("mark")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Mark([FromBody] MarkTeacherAttendanceDto dto)
        {
            var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var domain  = _mapper.Map<TeacherDailyAttendance>(dto);
            var result  = await _repo.MarkAsync(domain, userId);
            return Ok(_mapper.Map<TeacherDailyAttendanceDto>(result));
        }

        // ── POST api/TeacherDailyAttendance/checkin ──────────────────────────
        // Reception barcode scanner endpoint — no auth required so the scanner
        // terminal (kiosk) can post without a login session.
        // The barcode is matched against Teachers.RegistrationNo.
        [HttpPost("checkin")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckIn([FromBody] BarcodeCheckInDto dto)
        {
            var result = await _repo.CheckInByBarcodeAsync(
                dto.Barcode,
                (TeacherAttendanceStatus)dto.Status);

            if (result == null)
                return NotFound(new { message = $"No teacher found with barcode '{dto.Barcode}'." });

            return Ok(_mapper.Map<TeacherDailyAttendanceDto>(result));
        }

        // ── DELETE api/TeacherDailyAttendance/{id} ───────────────────────────
        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(_mapper.Map<TeacherDailyAttendanceDto>(deleted));
        }
    }
}




