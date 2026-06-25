using InstituteWebAPI.Data;
using InstituteWebAPI.Models.DTO.StudentLeaveRequest;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher")]
    public class StudentLeaveRequestsController : ControllerBase
    {
        private readonly RozhnInstituteDbContext db;
        private readonly ITeacherIdentityLinkRepository teacherIdentity;
        private readonly InstituteWebAPI.Services.Notifications.IAppNotificationService notifications;

        public StudentLeaveRequestsController(
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

        // ── GET /api/StudentLeaveRequests  (Admin: all; Teacher: own)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var query = db.StudentLeaveRequests
                .AsNoTracking()
                .Include(r => r.Student)
                .Include(r => r.CurrentClass).ThenInclude(cc => cc.Class)
                .Include(r => r.CurrentClass).ThenInclude(cc => cc.Slot)
                .Include(r => r.RequestedByTeacher)
                .AsQueryable();

            if (User.IsInRole("Teacher"))
            {
                var tid = await GetTeacherIdAsync();
                if (tid == null) return Forbid();
                query = query.Where(r => r.RequestedByTeacherID == tid.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LeaveRequestStatus>(status, true, out var s))
                query = query.Where(r => r.Status == s);

            var list = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            return Ok(list.Select(MapToDto));
        }

        // ── POST /api/StudentLeaveRequests  (Teacher only)
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(CreateLeaveRequestDto dto)
        {
            var teacherId = await GetTeacherIdAsync();
            if (teacherId == null) return Forbid();

            // Teacher must own this class
            var cc = await db.CurrentClasses.FindAsync(dto.CurrentClassID);
            if (cc == null || cc.TeacherID != teacherId) return Forbid();

            // Student must be enrolled in this class
            var enrolled = await db.ClassStudents
                .AnyAsync(cs => cs.CurrentClassID == dto.CurrentClassID && cs.StudentID == dto.StudentID);
            if (!enrolled) return BadRequest("Student is not enrolled in this class.");

            // No duplicate pending request
            var duplicate = await db.StudentLeaveRequests
                .AnyAsync(r => r.StudentID == dto.StudentID
                            && r.CurrentClassID == dto.CurrentClassID
                            && r.Status == LeaveRequestStatus.Pending);
            if (duplicate) return BadRequest("A pending dropout report already exists for this student.");

            // Resolve AdmissionID — use supplied value or auto-find the student's active admission
            var admissionId = dto.AdmissionID ?? await db.Admissions
                .Where(a => a.StudentID == dto.StudentID && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => (Guid?)a.AdmissionID)
                .FirstOrDefaultAsync();

            if (admissionId == null || admissionId == Guid.Empty)
                return BadRequest("No active admission found for this student.");

            var entity = new StudentLeaveRequest
            {
                StudentLeaveRequestID = Guid.NewGuid(),
                StudentID             = dto.StudentID,
                CurrentClassID        = dto.CurrentClassID,
                AdmissionID           = admissionId.Value,
                RequestedByTeacherID  = teacherId.Value,
                LeavingDate           = dto.LeavingDate,
                Reason                = dto.Reason,
                Status                = LeaveRequestStatus.Pending,
                CreatedAt             = DateTime.Now,
                UpdatedAt             = DateTime.Now,
            };

            db.StudentLeaveRequests.Add(entity);
            await db.SaveChangesAsync();

            // Reload for full nav props
            await db.Entry(entity).Reference(e => e.Student).LoadAsync();
            await db.Entry(entity).Reference(e => e.CurrentClass).LoadAsync();
            await db.Entry(entity.CurrentClass).Reference(cc2 => cc2.Class).LoadAsync();
            await db.Entry(entity).Reference(e => e.RequestedByTeacher).LoadAsync();

            await notifications.NotifyRoleAsync(
                "Admin",
                AppNotificationType.LeaveRequest,
                "New dropout report",
                $"{entity.RequestedByTeacher?.TeacherName ?? "A teacher"} reported that {entity.Student?.StudentName ?? "a student"} has dropped out.",
                "/leave-requests");

            return Ok(MapToDto(entity));
        }

        // ── PUT /api/StudentLeaveRequests/{id}/approve  (Admin only)
        [HttpPut("{id:Guid}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(Guid id, ReviewLeaveRequestDto dto)
        {
            var req = await db.StudentLeaveRequests
                .Include(r => r.Student)
                .Include(r => r.CurrentClass).ThenInclude(cc => cc.Class)
                .FirstOrDefaultAsync(r => r.StudentLeaveRequestID == id);

            if (req == null) return NotFound();
            if (req.Status != LeaveRequestStatus.Pending)
                return BadRequest("This request has already been reviewed.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Mark request as Approved
            req.Status          = LeaveRequestStatus.Approved;
            req.ReviewedByUserID = userId;
            req.ReviewedAt      = DateTime.Now;
            req.ReviewRemarks   = dto.ReviewRemarks;
            req.UpdatedAt       = DateTime.Now;

            // 2. Update Admissions: Status = Left, LeavingDate, IsActive = false
            var admission = await db.Admissions.FindAsync(req.AdmissionID);
            if (admission != null)
            {
                admission.Status      = AdmissionStatus.Left.ToString();
                admission.IsActive    = false;
                admission.LeavingDate = req.LeavingDate;
                admission.ModifiedAt  = DateTime.Now;
            }

            // 3. Remove student from ClassStudents (unenroll)
            var enrollment = await db.ClassStudents
                .FirstOrDefaultAsync(cs => cs.CurrentClassID == req.CurrentClassID
                                        && cs.StudentID == req.StudentID);
            if (enrollment != null)
                db.ClassStudents.Remove(enrollment);

            await db.SaveChangesAsync();
            return Ok(MapToDto(req));
        }

        // ── PUT /api/StudentLeaveRequests/{id}/reject  (Admin only)
        [HttpPut("{id:Guid}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(Guid id, ReviewLeaveRequestDto dto)
        {
            var req = await db.StudentLeaveRequests
                .Include(r => r.Student)
                .Include(r => r.CurrentClass).ThenInclude(cc => cc.Class)
                .FirstOrDefaultAsync(r => r.StudentLeaveRequestID == id);

            if (req == null) return NotFound();
            if (req.Status != LeaveRequestStatus.Pending)
                return BadRequest("This request has already been reviewed.");

            req.Status           = LeaveRequestStatus.Rejected;
            req.ReviewedByUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            req.ReviewedAt       = DateTime.Now;
            req.ReviewRemarks    = dto.ReviewRemarks;
            req.UpdatedAt        = DateTime.Now;

            await db.SaveChangesAsync();
            return Ok(MapToDto(req));
        }

        // ── GET /api/StudentLeaveRequests/pending-count  (Admin)
        [HttpGet("pending-count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PendingCount()
        {
            var count = await db.StudentLeaveRequests
                .CountAsync(r => r.Status == LeaveRequestStatus.Pending);
            return Ok(new { count });
        }

        // ── Mapping helper ───────────────────────────────────────────────────
        private static StudentLeaveRequestDto MapToDto(StudentLeaveRequest r) => new()
        {
            StudentLeaveRequestID   = r.StudentLeaveRequestID,
            StudentID               = r.StudentID,
            StudentName             = r.Student?.StudentName ?? "",
            RegistrationNo          = r.Student?.RegistrationNo ?? "",
            CurrentClassID          = r.CurrentClassID,
            ClassName               = r.CurrentClass?.Class?.ClassName ?? "",
            SlotName                = r.CurrentClass?.Slot?.SlotName ?? "",
            AdmissionID             = r.AdmissionID,
            RequestedByTeacherID    = r.RequestedByTeacherID,
            RequestedByTeacherName  = r.RequestedByTeacher?.TeacherName ?? "",
            LeavingDate             = r.LeavingDate,
            Reason                  = r.Reason,
            Status                  = r.Status.ToString(),
            ReviewedByUserID        = r.ReviewedByUserID,
            ReviewedAt              = r.ReviewedAt,
            ReviewRemarks           = r.ReviewRemarks,
            CreatedAt               = r.CreatedAt,
        };
    }
}
