using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace InstituteWebAPI.Controllers
{
    // Lets a teacher raise an ID-card request for a student in one of their own
    // classes. Admins manage/price/deliver them via AdminCardRequestController.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class TeacherCardRequestController : ControllerBase
    {
        private readonly RozhnInstituteDbContext db;
        private readonly ITeacherIdentityLinkRepository teacherIdentity;
        private readonly InstituteWebAPI.Services.Notifications.IAppNotificationService notifications;
        private readonly InstituteWebAPI.Services.FeeManagement.IFeeManagementService feeService;

        public TeacherCardRequestController(
            RozhnInstituteDbContext db,
            ITeacherIdentityLinkRepository teacherIdentity,
            InstituteWebAPI.Services.Notifications.IAppNotificationService notifications,
            InstituteWebAPI.Services.FeeManagement.IFeeManagementService feeService)
        {
            this.db = db;
            this.teacherIdentity = teacherIdentity;
            this.notifications = notifications;
            this.feeService = feeService;
        }

        private static readonly HashSet<string> CardTypes =
            new(StringComparer.OrdinalIgnoreCase) { "New", "Replacement" };
        private static string NormType(string? t) =>
            CardTypes.FirstOrDefault(x => x.Equals((t ?? "").Trim(), StringComparison.OrdinalIgnoreCase)) ?? "New";

        private async Task<Guid?> GetTeacherIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return null;
            return await teacherIdentity.GetTeacherIdForUserIdAsync(userId);
        }

        public class TeacherCardRequestRowDto
        {
            public Guid CardRequestID { get; set; }
            public Guid StudentID { get; set; }
            public string? StudentName { get; set; }
            public string? RegistrationNo { get; set; }
            public string CardType { get; set; } = "New";
            public string Status { get; set; } = "Requested";
            public DateTime RequestDate { get; set; }
            public string? Notes { get; set; }
        }

        public class CreateTeacherCardRequestDto
        {
            [Required] public Guid StudentID { get; set; }
            public string? CardType { get; set; }
            public string? Notes { get; set; }
        }

        // ── GET /api/TeacherCardRequest  (the teacher's own requests) ──────────
        [HttpGet]
        public async Task<IActionResult> GetMine()
        {
            var teacherId = await GetTeacherIdAsync();
            if (teacherId == null) return Forbid();

            var rows = await db.CardRequests.AsNoTracking()
                .Where(c => c.RequestedByTeacherID == teacherId.Value)
                .Include(c => c.Student)
                .OrderByDescending(c => c.RequestDate)
                .Select(c => new TeacherCardRequestRowDto
                {
                    CardRequestID  = c.CardRequestID,
                    StudentID      = c.StudentID,
                    StudentName    = c.Student != null ? c.Student.StudentName : null,
                    RegistrationNo = c.Student != null ? c.Student.RegistrationNo : null,
                    CardType       = c.CardType,
                    Status         = c.Status,
                    RequestDate    = c.RequestDate,
                    Notes          = c.Notes,
                })
                .ToListAsync();

            return Ok(rows);
        }

        // ── POST /api/TeacherCardRequest  (create for own student) ─────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeacherCardRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var teacherId = await GetTeacherIdAsync();
            if (teacherId == null) return Forbid();

            // The student must be enrolled in a class taught by this teacher.
            var ownsStudent = await db.ClassStudents.AnyAsync(cs =>
                cs.StudentID == dto.StudentID &&
                cs.Status == "Enrolled" &&
                cs.CurrentClass.TeacherID == teacherId.Value);
            if (!ownsStudent)
                return BadRequest(new { message = "This student is not in one of your classes." });

            // Avoid stacking duplicate open requests.
            var openExists = await db.CardRequests.AnyAsync(c =>
                c.StudentID == dto.StudentID && c.Status == "Requested");
            if (openExists)
                return BadRequest(new { message = "An open card request already exists for this student." });

            var entity = new CardRequest
            {
                CardRequestID        = Guid.NewGuid(),
                StudentID            = dto.StudentID,
                CardType             = NormType(dto.CardType),
                Amount               = 0m,                       // admin sets the fee on payment
                Status               = "Requested",
                RequestDate          = DateTime.UtcNow,
                Notes                = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                RequestedByTeacherID = teacherId.Value,
                CreatedOn            = DateTime.UtcNow,
            };
            db.CardRequests.Add(entity);
            await db.SaveChangesAsync();

            // Generate the card fee (configured amount) in the fee module so it can be collected.
            try { await feeService.TryGenerateCardFeeAsync(dto.StudentID, 0m); }
            catch { /* never block the card request on fee generation */ }

            var studentName = await db.Students
                .Where(s => s.StudentID == dto.StudentID)
                .Select(s => s.StudentName)
                .FirstOrDefaultAsync();
            var teacherName = await db.Teachers
                .Where(t => t.TeacherID == teacherId.Value)
                .Select(t => t.TeacherName)
                .FirstOrDefaultAsync();

            await notifications.NotifyRoleAsync(
                "Admin",
                AppNotificationType.CardRequest,
                "New ID card request",
                $"{teacherName ?? "A teacher"} requested a {entity.CardType} ID card for {studentName ?? "a student"}.",
                "/card-requests");

            return Ok(new { entity.CardRequestID });
        }
    }
}
