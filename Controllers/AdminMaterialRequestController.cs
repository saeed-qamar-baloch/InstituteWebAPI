using InstituteWebAPI.Data;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Controllers
{
    // Admin view of teacher material requests. Defaults to showing only Pending
    // requests — Approved/Fulfilled/Rejected requests are "inactive" history,
    // only shown when a status filter (or "All") is explicitly requested.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminMaterialRequestController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly InstituteWebAPI.Services.Notifications.IAppNotificationService notifications;

        public AdminMaterialRequestController(
            RozhnInstituteDbContext dbContext,
            InstituteWebAPI.Services.Notifications.IAppNotificationService notifications)
        {
            this.dbContext = dbContext;
            this.notifications = notifications;
        }

        private static readonly HashSet<string> Statuses =
            new(StringComparer.OrdinalIgnoreCase) { "Pending", "Approved", "Fulfilled", "Rejected" };
        private static string NormStatus(string? s) =>
            Statuses.FirstOrDefault(x => x.Equals((s ?? "").Trim(), StringComparison.OrdinalIgnoreCase)) ?? "Pending";

        public class MaterialRequestDto
        {
            public Guid MaterialRequestID { get; set; }
            public Guid TeacherID { get; set; }
            public string? TeacherName { get; set; }
            public string? TeacherRegistrationNo { get; set; }
            public string MaterialName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public string? Description { get; set; }
            public string Status { get; set; } = "Pending";
            public DateTime RequestDate { get; set; }
            public DateTime? ReviewedOn { get; set; }
            public DateTime? FulfilledOn { get; set; }
            public string? AdminNote { get; set; }
        }

        public class SetMaterialStatusDto
        {
            public string? Status { get; set; }
            public string? AdminNote { get; set; }
        }

        // ── GET /api/AdminMaterialRequest?status=Pending|Approved|Fulfilled|Rejected|All ──
        // No status (or omitted) => Pending only, matching the admin's default "what needs action" view.
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var q = dbContext.MaterialRequests.AsNoTracking().Include(m => m.Teacher).AsQueryable();

            if (string.IsNullOrWhiteSpace(status))
            {
                q = q.Where(m => m.Status == "Pending");
            }
            else if (!status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var norm = NormStatus(status);
                q = q.Where(m => m.Status == norm);
            }
            // status == "All" => no filter, used by the admin's history view.

            var rows = await q
                .OrderByDescending(m => m.RequestDate)
                .Select(m => new MaterialRequestDto
                {
                    MaterialRequestID     = m.MaterialRequestID,
                    TeacherID              = m.TeacherID,
                    TeacherName            = m.Teacher != null ? m.Teacher.TeacherName : null,
                    TeacherRegistrationNo  = m.Teacher != null ? m.Teacher.RegistrationNo : null,
                    MaterialName           = m.MaterialName,
                    Quantity               = m.Quantity,
                    Description            = m.Description,
                    Status                 = m.Status,
                    RequestDate            = m.RequestDate,
                    ReviewedOn             = m.ReviewedOn,
                    FulfilledOn            = m.FulfilledOn,
                    AdminNote              = m.AdminNote,
                })
                .ToListAsync();

            return Ok(rows);
        }

        // Pending count, for a dashboard / sidebar badge.
        [HttpGet("pending-count")]
        public async Task<IActionResult> PendingCount()
        {
            var count = await dbContext.MaterialRequests.CountAsync(m => m.Status == "Pending");
            return Ok(new { count });
        }

        // Set status; stamps ReviewedOn / FulfilledOn automatically.
        // Pending -> Approved -> Fulfilled, or Pending -> Rejected.
        [HttpPut("{id:Guid}/status")]
        public async Task<IActionResult> SetStatus(Guid id, [FromBody] SetMaterialStatusDto dto)
        {
            var e = await dbContext.MaterialRequests.FirstOrDefaultAsync(m => m.MaterialRequestID == id);
            if (e == null) return NotFound();

            var status = NormStatus(dto.Status);
            e.Status = status;
            if (!string.IsNullOrWhiteSpace(dto.AdminNote)) e.AdminNote = dto.AdminNote.Trim();

            if (status == "Approved" || status == "Rejected")
            {
                e.ReviewedOn ??= DateTime.UtcNow;
            }
            if (status == "Fulfilled")
            {
                e.ReviewedOn ??= DateTime.UtcNow;
                e.FulfilledOn ??= DateTime.UtcNow;
            }
            if (status == "Pending")
            {
                e.ReviewedOn = null;
                e.FulfilledOn = null;
            }

            await dbContext.SaveChangesAsync();
            return Ok(new { e.MaterialRequestID, e.Status, e.ReviewedOn, e.FulfilledOn });
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var e = await dbContext.MaterialRequests.FirstOrDefaultAsync(m => m.MaterialRequestID == id);
            if (e == null) return NotFound();
            dbContext.MaterialRequests.Remove(e);
            await dbContext.SaveChangesAsync();
            return Ok(new { id });
        }
    }
}
