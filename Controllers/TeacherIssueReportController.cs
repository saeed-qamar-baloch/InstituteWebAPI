using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebAPI.Services.Notifications;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace InstituteWebAPI.Controllers
{
    /// <summary>
    /// Lets a teacher submit an issue report about a class or a student in that class.
    /// Teachers only see their own reports; admins use AdminIssueReportController.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class TeacherIssueReportController : ControllerBase
    {
        private readonly RozhnInstituteDbContext db;
        private readonly ITeacherIdentityLinkRepository teacherIdentity;
        private readonly IAppNotificationService notifications;

        public TeacherIssueReportController(
            RozhnInstituteDbContext db,
            ITeacherIdentityLinkRepository teacherIdentity,
            IAppNotificationService notifications)
        {
            this.db              = db;
            this.teacherIdentity = teacherIdentity;
            this.notifications   = notifications;
        }

        // ── DTOs ─────────────────────────────────────────────────────────────

        public class CreateIssueReportDto
        {
            [Required]
            public Guid CurrentClassId { get; set; }

            // Null = issue is about the class in general, not a specific student.
            public Guid? StudentId { get; set; }

            [Required]
            [Range(1, 4)]
            public int IssueType { get; set; }

            [Required, MaxLength(1000)]
            public string Description { get; set; } = string.Empty;
        }

        public class IssueReportRowDto
        {
            public Guid   IssueId      { get; set; }
            public string ClassName    { get; set; } = string.Empty;
            public string? StudentName { get; set; }
            public string IssueType    { get; set; } = string.Empty;
            public string Description  { get; set; } = string.Empty;
            public string Status       { get; set; } = string.Empty;
            public string? AdminNotes  { get; set; }
            public DateTime CreatedAt  { get; set; }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task<Guid?> GetTeacherIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return null;
            return await teacherIdentity.GetTeacherIdForUserIdAsync(userId);
        }

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

        // ── GET /api/TeacherIssueReport  ─────────────────────────────────────
        // Returns only the calling teacher's own submitted reports.
        [HttpGet]
        public async Task<IActionResult> GetMine()
        {
            var teacherId = await GetTeacherIdAsync();
            if (teacherId == null) return Forbid();

            var rows = await db.IssueReports
                .AsNoTracking()
                .Where(r => r.TeacherId == teacherId.Value)
                .Include(r => r.CurrentClass).ThenInclude(cc => cc.Class)
                .Include(r => r.Student)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new IssueReportRowDto
                {
                    IssueId     = r.IssueId,
                    ClassName   = r.CurrentClass.Class.ClassName,
                    StudentName = r.Student != null ? r.Student.StudentName : null,
                    IssueType   = IssueTypeLabel(r.IssueType),
                    Description = r.Description,
                    Status      = StatusLabel(r.Status),
                    AdminNotes  = r.AdminNotes,
                    CreatedAt   = r.CreatedAt,
                })
                .ToListAsync();

            return Ok(rows);
        }

        // ── POST /api/TeacherIssueReport  ────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateIssueReportDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var teacherId = await GetTeacherIdAsync();
            if (teacherId == null) return Forbid();

            // Verify the teacher actually owns this class.
            var ownsClass = await db.CurrentClasses
                .AnyAsync(cc => cc.CurrentClassID == dto.CurrentClassId
                             && cc.TeacherID       == teacherId.Value);
            if (!ownsClass) return Forbid();

            // If a student was specified, verify they belong to this class.
            if (dto.StudentId.HasValue)
            {
                var studentInClass = await db.ClassStudents
                    .AnyAsync(cs => cs.CurrentClassID == dto.CurrentClassId
                                 && cs.StudentID      == dto.StudentId.Value);
                if (!studentInClass)
                    return BadRequest("The selected student is not enrolled in this class.");
            }

            var issueType = (IssueType)dto.IssueType;
            var entity = new TeacherIssueReport
            {
                IssueId        = Guid.NewGuid(),
                TeacherId      = teacherId.Value,
                CurrentClassId = dto.CurrentClassId,
                StudentId      = dto.StudentId,
                IssueType      = issueType,
                Description    = dto.Description.Trim(),
                Status         = IssueStatus.Open,
                CreatedAt      = DateTime.UtcNow,
                UpdatedAt      = DateTime.UtcNow,
            };

            db.IssueReports.Add(entity);
            await db.SaveChangesAsync();

            // Notify admins so they can act on it promptly.
            var teacherName = await db.Teachers
                .Where(t => t.TeacherID == teacherId.Value)
                .Select(t => t.TeacherName)
                .FirstOrDefaultAsync();

            await notifications.NotifyRoleAsync(
                "Admin",
                AppNotificationType.IssueReport,
                "New Issue Report",
                $"{teacherName ?? "A teacher"} reported: {IssueTypeLabel(issueType)}.",
                "/issue-reports");

            return Ok(new { entity.IssueId });
        }
    }
}
