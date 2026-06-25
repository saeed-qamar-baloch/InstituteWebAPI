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
    // Lets a teacher request teaching material (books, wordlists, supplies, etc.).
    // Admins review/approve/fulfil them via AdminMaterialRequestController.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class TeacherMaterialRequestController : ControllerBase
    {
        private readonly RozhnInstituteDbContext db;
        private readonly ITeacherIdentityLinkRepository teacherIdentity;
        private readonly InstituteWebAPI.Services.Notifications.IAppNotificationService notifications;

        public TeacherMaterialRequestController(
            RozhnInstituteDbContext db,
            ITeacherIdentityLinkRepository teacherIdentity,
            InstituteWebAPI.Services.Notifications.IAppNotificationService notifications)
        {
            this.db = db;
            this.teacherIdentity = teacherIdentity;
            this.notifications = notifications;
        }

        private async Task<Guid?> GetTeacherIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return null;
            return await teacherIdentity.GetTeacherIdForUserIdAsync(userId);
        }

        public class TeacherMaterialRequestRowDto
        {
            public Guid MaterialRequestID { get; set; }
            public string MaterialName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public string? Description { get; set; }
            public string Status { get; set; } = "Pending";
            public DateTime RequestDate { get; set; }
            public string? AdminNote { get; set; }
        }

        public class CreateMaterialRequestDto
        {
            [Required, MaxLength(200)] public string MaterialName { get; set; } = string.Empty;
            [Range(1, int.MaxValue)] public int Quantity { get; set; } = 1;
            [MaxLength(1000)] public string? Description { get; set; }
        }

        // ── GET /api/TeacherMaterialRequest  (the teacher's own requests) ──────
        [HttpGet]
        public async Task<IActionResult> GetMine()
        {
            var teacherId = await GetTeacherIdAsync();
            if (teacherId == null) return Forbid();

            var rows = await db.MaterialRequests.AsNoTracking()
                .Where(m => m.TeacherID == teacherId.Value)
                .OrderByDescending(m => m.RequestDate)
                .Select(m => new TeacherMaterialRequestRowDto
                {
                    MaterialRequestID = m.MaterialRequestID,
                    MaterialName      = m.MaterialName,
                    Quantity          = m.Quantity,
                    Description       = m.Description,
                    Status            = m.Status,
                    RequestDate       = m.RequestDate,
                    AdminNote         = m.AdminNote,
                })
                .ToListAsync();

            return Ok(rows);
        }

        // ── POST /api/TeacherMaterialRequest  (create a new request) ───────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMaterialRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var teacherId = await GetTeacherIdAsync();
            if (teacherId == null) return Forbid();

            var entity = new MaterialRequest
            {
                MaterialRequestID = Guid.NewGuid(),
                TeacherID         = teacherId.Value,
                MaterialName      = dto.MaterialName.Trim(),
                Quantity          = dto.Quantity < 1 ? 1 : dto.Quantity,
                Description       = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                Status            = "Pending",
                RequestDate       = DateTime.UtcNow,
                CreatedOn         = DateTime.UtcNow,
            };
            db.MaterialRequests.Add(entity);
            await db.SaveChangesAsync();

            var teacherName = await db.Teachers
                .Where(t => t.TeacherID == teacherId.Value)
                .Select(t => t.TeacherName)
                .FirstOrDefaultAsync();

            await notifications.NotifyRoleAsync(
                "Admin",
                AppNotificationType.MaterialRequest,
                "New material request",
                $"{teacherName ?? "A teacher"} requested {entity.Quantity}x \"{entity.MaterialName}\".",
                "/material-requests");

            return Ok(new { entity.MaterialRequestID });
        }
    }
}
