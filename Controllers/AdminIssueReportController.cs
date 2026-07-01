using InstituteWebAPI.Data;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Controllers
{
    /// <summary>
    /// Admin view of teacher issue reports.
    /// Defaults to Open reports — resolved/dismissed history is shown on explicit request.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminIssueReportController : ControllerBase
    {
        private readonly RozhnInstituteDbContext db;

        public AdminIssueReportController(RozhnInstituteDbContext db)
        {
            this.db = db;
        }

        // ── DTOs ──────────────────────────────────────────────────────────────

        public class IssueReportDto
        {
            public Guid    IssueId       { get; set; }
            public string  TeacherName   { get; set; } = string.Empty;
            public string? TeacherRegNo  { get; set; }
            public string  ClassName     { get; set; } = string.Empty;
            public string? StudentName   { get; set; }
            public string? StudentRegNo  { get; set; }
            public string  IssueType     { get; set; } = string.Empty;
            public int     IssueTypeId   { get; set; }
            public string  Description   { get; set; } = string.Empty;
            public string  Status        { get; set; } = string.Empty;
            public int     StatusId      { get; set; }
            public string? AdminNotes    { get; set; }
            public DateTime CreatedAt    { get; set; }
            public DateTime UpdatedAt    { get; set; }
        }

        public class UpdateIssueStatusDto
        {
            [Required]
            [Range(1, 4)]
            public int Status { get; set; }

            [MaxLength(500)]
            public string? AdminNotes { get; set; }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string IssueTypeLabel(IssueType t) => t switch
        {
            IssueType.InfoCorrection             => "Information Correction",
            IssueType.StudentDoesntBelongToClass => "Student Doesn't Belong to Class",
            IssueType.StudentNotListedInClass    => "Student Not Listed in Class",
            IssueType.OtherIssue                 => "Other Issue",
            _                                    => t.ToString(),
        };

        private static string StatusLabel(IssueStatus s) => s switch
        {
            IssueStatus.Open       => "Open",
            IssueStatus.InProgress => "In Progress",
            IssueStatus.Resolved   => "Resolved",
            IssueStatus.Dismissed  => "Dismissed",
            _                      => s.ToString(),
        };

        private IssueReportDto MapRow(TeacherIssueReport r) => new()
        {
            IssueId      = r.IssueId,
            TeacherName  = r.Teacher?.TeacherName ?? string.Empty,
            TeacherRegNo = r.Teacher?.RegistrationNo,
            ClassName    = r.CurrentClass?.Class?.ClassName ?? string.Empty,
            StudentName  = r.Student?.StudentName,
            StudentRegNo = r.Student?.RegistrationNo,
            IssueType    = IssueTypeLabel(r.IssueType),
            IssueTypeId  = (int)r.IssueType,
            Description  = r.Description,
            Status       = StatusLabel(r.Status),
            StatusId     = (int)r.Status,
            AdminNotes   = r.AdminNotes,
            CreatedAt    = r.CreatedAt,
            UpdatedAt    = r.UpdatedAt,
        };

        private IQueryable<TeacherIssueReport> BaseQuery() =>
            db.IssueReports
                .AsNoTracking()
                .Include(r => r.Teacher)
                .Include(r => r.CurrentClass).ThenInclude(cc => cc.Class)
                .Include(r => r.Student);

        // ── GET /api/AdminIssueReport?status=open|inprogress|resolved|dismissed|all ──
        // No status param → Open only (what needs action).
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var q = BaseQuery();

            if (string.IsNullOrWhiteSpace(status) || status.Equals("open", StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r => r.Status == IssueStatus.Open);
            }
            else if (status.Equals("inprogress", StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r => r.Status == IssueStatus.InProgress);
            }
            else if (status.Equals("resolved", StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r => r.Status == IssueStatus.Resolved);
            }
            else if (status.Equals("dismissed", StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(r => r.Status == IssueStatus.Dismissed);
            }
            // "all" → no filter

            var rows = await q
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(rows.Select(MapRow));
        }

        // ── GET /api/AdminIssueReport/open-count  ────────────────────────────
        [HttpGet("open-count")]
        public async Task<IActionResult> OpenCount()
        {
            var count = await db.IssueReports
                .CountAsync(r => r.Status == IssueStatus.Open || r.Status == IssueStatus.InProgress);
            return Ok(new { count });
        }

        // ── PUT /api/AdminIssueReport/{id}/status  ───────────────────────────
        [HttpPut("{id:Guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateIssueStatusDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = await db.IssueReports.FirstOrDefaultAsync(r => r.IssueId == id);
            if (entity == null) return NotFound();

            entity.Status    = (IssueStatus)dto.Status;
            entity.UpdatedAt = DateTime.UtcNow;

            if (dto.AdminNotes != null)
                entity.AdminNotes = dto.AdminNotes.Trim() == string.Empty ? null : dto.AdminNotes.Trim();

            await db.SaveChangesAsync();
            return Ok(new { entity.IssueId, Status = StatusLabel(entity.Status), entity.AdminNotes });
        }

        // ── DELETE /api/AdminIssueReport/{id}  ───────────────────────────────
        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await db.IssueReports.FirstOrDefaultAsync(r => r.IssueId == id);
            if (entity == null) return NotFound();
            db.IssueReports.Remove(entity);
            await db.SaveChangesAsync();
            return Ok(new { id });
        }
    }
}
