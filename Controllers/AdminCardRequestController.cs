using InstituteWebAPI.Data;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminCardRequestController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly InstituteWebAPI.Services.Notifications.IAppNotificationService notifications;
        private readonly InstituteWebAPI.Services.FeeManagement.IFeeManagementService feeService;
        public AdminCardRequestController(
            RozhnInstituteDbContext dbContext,
            InstituteWebAPI.Services.Notifications.IAppNotificationService notifications,
            InstituteWebAPI.Services.FeeManagement.IFeeManagementService feeService)
        {
            this.dbContext = dbContext;
            this.notifications = notifications;
            this.feeService = feeService;
        }

        private static readonly HashSet<string> Statuses =
            new(StringComparer.OrdinalIgnoreCase) { "Requested", "Paid", "Delivered" };
        private static readonly HashSet<string> CardTypes =
            new(StringComparer.OrdinalIgnoreCase) { "New", "Replacement" };

        public class CardRequestDto
        {
            public Guid CardRequestID { get; set; }
            public Guid StudentID { get; set; }
            public string? StudentName { get; set; }
            public string? RegistrationNo { get; set; }
            public string? FatherName { get; set; }
            public string? Picture { get; set; }
            public string? ClassName { get; set; }
            public string CardType { get; set; } = "New";
            public decimal Amount { get; set; }
            public string Status { get; set; } = "Requested";
            public DateTime RequestDate { get; set; }
            public DateTime? PaidOn { get; set; }
            public DateTime? DeliveredOn { get; set; }
            public string? Notes { get; set; }
            public string? RequestedByTeacherName { get; set; }
        }

        public class SaveCardRequestDto
        {
            [Required] public Guid StudentID { get; set; }
            public string? CardType { get; set; }
            [Range(0, double.MaxValue)] public decimal Amount { get; set; }
            public string? Notes { get; set; }
        }

        public class CardSetStatusDto { public string? Status { get; set; } }

        private static string NormType(string? t) =>
            CardTypes.FirstOrDefault(x => x.Equals((t ?? "").Trim(), StringComparison.OrdinalIgnoreCase)) ?? "New";
        private static string NormStatus(string? s) =>
            Statuses.FirstOrDefault(x => x.Equals((s ?? "").Trim(), StringComparison.OrdinalIgnoreCase)) ?? "Requested";

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] Guid? studentId, [FromQuery] string? search)
        {
            var q = dbContext.CardRequests.AsNoTracking().Include(c => c.Student).AsQueryable();
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(c => c.Status == status);
            if (studentId.HasValue) q = q.Where(c => c.StudentID == studentId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(c => c.Student.StudentName.Contains(s) || c.Student.RegistrationNo.Contains(s) || c.Student.FatherName.Contains(s));
            }

            var rows = await q
                .OrderByDescending(c => c.RequestDate)
                .Select(c => new CardRequestDto
                {
                    CardRequestID = c.CardRequestID,
                    StudentID = c.StudentID,
                    StudentName = c.Student != null ? c.Student.StudentName : null,
                    RegistrationNo = c.Student != null ? c.Student.RegistrationNo : null,
                    FatherName = c.Student != null ? c.Student.FatherName : null,
                    Picture = c.Student != null ? c.Student.Picture : null,
                    ClassName = c.Student.ClassStudents
                        .Where(cs => cs.Status == "Enrolled")
                        .Select(cs => cs.CurrentClass.Class.ClassName)
                        .FirstOrDefault(),
                    CardType = c.CardType,
                    Amount = c.Amount,
                    Status = c.Status,
                    RequestDate = c.RequestDate,
                    PaidOn = c.PaidOn,
                    DeliveredOn = c.DeliveredOn,
                    Notes = c.Notes,
                    RequestedByTeacherName = c.RequestedByTeacher != null ? c.RequestedByTeacher.TeacherName : null,
                })
                .ToListAsync();
            return Ok(rows);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveCardRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var exists = await dbContext.Students.AnyAsync(s => s.StudentID == dto.StudentID);
            if (!exists) return BadRequest(new { message = "Student not found." });

            var entity = new CardRequest
            {
                CardRequestID = Guid.NewGuid(),
                StudentID = dto.StudentID,
                CardType = NormType(dto.CardType),
                Amount = dto.Amount,
                Status = "Requested",
                RequestDate = DateTime.UtcNow,
                Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                CreatedOn = DateTime.UtcNow,
            };
            dbContext.CardRequests.Add(entity);
            await dbContext.SaveChangesAsync();

            // Generate the matching card fee in the fee module so it can be collected.
            bool feeGenerated = false;
            try
            {
                var fee = await feeService.TryGenerateCardFeeAsync(dto.StudentID, dto.Amount);
                feeGenerated = fee != null;
            }
            catch { /* never block the card request on fee generation */ }

            var studentName = await dbContext.Students
                .Where(s => s.StudentID == dto.StudentID)
                .Select(s => s.StudentName)
                .FirstOrDefaultAsync();
            await notifications.NotifyRoleAsync(
                "Admin",
                AppNotificationType.CardRequest,
                "New ID card request",
                $"{entity.CardType} ID card requested for {studentName ?? "a student"}.",
                "/card-requests");

            return Ok(new { entity.CardRequestID, feeGenerated });
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SaveCardRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var e = await dbContext.CardRequests.FirstOrDefaultAsync(c => c.CardRequestID == id);
            if (e == null) return NotFound();
            e.StudentID = dto.StudentID;
            e.CardType = NormType(dto.CardType);
            e.Amount = dto.Amount;
            e.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
            await dbContext.SaveChangesAsync();
            return Ok(new { e.CardRequestID });
        }

        // Set status; stamps PaidOn / DeliveredOn automatically.
        [HttpPut("{id:Guid}/status")]
        public async Task<IActionResult> SetStatus(Guid id, [FromBody] CardSetStatusDto dto)
        {
            var e = await dbContext.CardRequests.FirstOrDefaultAsync(c => c.CardRequestID == id);
            if (e == null) return NotFound();

            var status = NormStatus(dto.Status);
            e.Status = status;
            if (status == "Paid" && e.PaidOn == null) e.PaidOn = DateTime.UtcNow;
            if (status == "Delivered")
            {
                if (e.PaidOn == null) e.PaidOn = DateTime.UtcNow;
                if (e.DeliveredOn == null) e.DeliveredOn = DateTime.UtcNow;
            }
            if (status == "Requested") { e.PaidOn = null; e.DeliveredOn = null; }

            await dbContext.SaveChangesAsync();
            return Ok(new { e.CardRequestID, e.Status, e.PaidOn, e.DeliveredOn });
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var e = await dbContext.CardRequests.FirstOrDefaultAsync(c => c.CardRequestID == id);
            if (e == null) return NotFound();
            dbContext.CardRequests.Remove(e);
            await dbContext.SaveChangesAsync();
            return Ok(new { id });
        }
    }
}
