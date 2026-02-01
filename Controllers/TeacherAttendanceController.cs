using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebAPI.Models.DTO.Attendance;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherAttendanceController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly ICurrentClassRepository currentClassRepository;
        private readonly ITeacherIdentityLinkRepository teacherIdentity;

        public TeacherAttendanceController(
            RozhnInstituteDbContext dbContext,
            ICurrentClassRepository currentClassRepository,
            ITeacherIdentityLinkRepository teacherIdentity)
        {
            this.dbContext = dbContext;
            this.currentClassRepository = currentClassRepository;
            this.teacherIdentity = teacherIdentity;
        }

        private static DateTime NormalizeDate(DateTime d) => d.Date;

        private async Task<Guid?> GetTeacherIdFromTokenAsync()
        {
            // Primary mapping: IdentityUser.Id -> Teachers.RegistrationNo (as per TeacherIdentityLinkRepository)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var byUserId = await teacherIdentity.GetTeacherIdForUserIdAsync(userId);
                if (byUserId.HasValue) return byUserId;
            }

            // Fallback mapping: some tokens may have teacher registration no in Name or a custom claim.
            var regNo = User.FindFirstValue("RegistrationNo")
                        ?? User.FindFirstValue(ClaimTypes.Name)
                        ?? User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(regNo)) return null;

            return await dbContext.Teachers
                .AsNoTracking()
                .Where(t => t.RegistrationNo == regNo)
                .Select(t => (Guid?)t.TeacherID)
                .FirstOrDefaultAsync();
        }

        private async Task<bool> TeacherOwnsCurrentClass(Guid currentClassId)
        {
            var currentClass = await currentClassRepository.GetAsync(currentClassId);
            if (currentClass == null) return false;

            var teacherIdFromToken = await GetTeacherIdFromTokenAsync();
            if (teacherIdFromToken == null) return false;

            return currentClass.TeacherID == teacherIdFromToken;
        }

        // Fetch attendance sheet for a date + class. Returns enrolled students with existing statuses (for edit).
        [HttpGet("sheet")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetSheet([FromQuery] Guid currentClassId, [FromQuery] DateTime date)
        {
            if (currentClassId == Guid.Empty) return BadRequest("currentClassId is required.");

            var day = NormalizeDate(date);

            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(currentClassId);
                if (!owns) return Forbid();
            }

            var students = await dbContext.ClassStudents
                .AsNoTracking()
                .Include(cs => cs.Student)
                .Where(cs => cs.CurrentClassID == currentClassId && cs.Status == "Enrolled")
                .Select(cs => new { cs.StudentID, cs.Student.RegistrationNo, cs.Student.StudentName, cs.Student.FatherName })
                .OrderBy(x => x.StudentName)
                .ToListAsync();

            var studentIds = students.Select(s => s.StudentID).ToList();

            var existing = await dbContext.StudentAttendances
                .AsNoTracking()
                .Where(a => a.CurrentClassID == currentClassId && a.AttendanceDate == day && studentIds.Contains(a.StudentID))
                .Select(a => new { a.StudentID, a.Status })
                .ToListAsync();

            var existingLookup = existing.ToDictionary(x => x.StudentID, x => x.Status);

            var dto = new AttendanceSheetDto
            {
                Date = day,
                CurrentClassID = currentClassId,
                Students = students.Select(s => new AttendanceStudentRowDto
                {
                    StudentID = s.StudentID,
                    RegistrationNo = s.RegistrationNo,
                    StudentName = s.StudentName,
                    FatherName = s.FatherName,
                    Status = existingLookup.TryGetValue(s.StudentID, out var st) ? st : AttendanceStatus.Present
                }).ToList()
            };

            return Ok(dto);
        }

        // Upsert attendance for a date + class (creates missing, updates existing)
        [HttpPost("save")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Save([FromBody] SaveAttendanceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.CurrentClassID == Guid.Empty) return BadRequest("CurrentClassID is required.");

            var day = NormalizeDate(dto.Date);

            if (User.IsInRole("Teacher"))
            {
                var owns = await TeacherOwnsCurrentClass(dto.CurrentClassID);
                if (!owns) return Forbid();
            }

            var teacherId = await GetTeacherIdFromTokenAsync();
            if (teacherId == null) return Forbid();

            var studentIds = dto.Students.Select(s => s.StudentID).Distinct().ToList();

            // Validate students are enrolled in class
            var allowed = await dbContext.ClassStudents
                .AsNoTracking()
                .Where(cs => cs.CurrentClassID == dto.CurrentClassID && cs.Status == "Enrolled" && studentIds.Contains(cs.StudentID))
                .Select(cs => cs.StudentID)
                .ToListAsync();

            if (allowed.Count != studentIds.Count)
                return BadRequest("One or more students are not enrolled in the selected class.");

            var existing = await dbContext.StudentAttendances
                .Where(a => a.CurrentClassID == dto.CurrentClassID && a.AttendanceDate == day && studentIds.Contains(a.StudentID))
                .ToListAsync();

            var existingByStudent = existing.ToDictionary(x => x.StudentID, x => x);

            foreach (var s in dto.Students)
            {
                if (existingByStudent.TryGetValue(s.StudentID, out var row))
                {
                    row.Status = s.Status;
                    row.MarkedByTeacherID = teacherId.Value;
                    row.UpdatedOn = DateTime.UtcNow;
                }
                else
                {
                    dbContext.StudentAttendances.Add(new StudentAttendance
                    {
                        StudentAttendanceID = Guid.NewGuid(),
                        AttendanceDate = day,
                        CurrentClassID = dto.CurrentClassID,
                        StudentID = s.StudentID,
                        Status = s.Status,
                        MarkedByTeacherID = teacherId.Value,
                        CreatedOn = DateTime.UtcNow
                    });
                }
            }

            await dbContext.SaveChangesAsync();
            return Ok("Attendance saved.");
        }
    }
}
