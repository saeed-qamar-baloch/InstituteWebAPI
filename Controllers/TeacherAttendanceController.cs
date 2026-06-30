using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebAPI.Models.DTO.Attendance;
using InstituteWebAPI.Services.TermContext;
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
        private readonly ITermContext termContext;

        public TeacherAttendanceController(
            RozhnInstituteDbContext dbContext,
            ICurrentClassRepository currentClassRepository,
            ITeacherIdentityLinkRepository teacherIdentity,
            ITermContext termContext)
        {
            this.dbContext = dbContext;
            this.currentClassRepository = currentClassRepository;
            this.teacherIdentity = teacherIdentity;
            this.termContext = termContext;
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

        // Returns classes for the current active term only.
        // Falls back to the most recently started term if none is marked active.
        [HttpGet("my-classes")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetMyClasses()
        {
            // 1) Try the explicitly active term first.
            //    If multiple are flagged active (legacy data), pick the most-recently-started.
            var activeTermId = await dbContext.Term
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.TermStart)
                .Select(t => (Guid?)t.TermID)
                .FirstOrDefaultAsync();

            // 2) Fall back to the most recently started term (not "all active classes")
            if (!activeTermId.HasValue)
            {
                activeTermId = await dbContext.Term
                    .AsNoTracking()
                    .OrderByDescending(t => t.TermStart)
                    .Select(t => (Guid?)t.TermID)
                    .FirstOrDefaultAsync();
            }

            IQueryable<CurrentClass> query = dbContext.CurrentClasses
                .AsNoTracking()
                .Include(cc => cc.Class)
                .Include(cc => cc.Teacher)
                .Include(cc => cc.Section)
                .Include(cc => cc.Term);

            // Always filter to the resolved term — never show classes across all terms
            if (activeTermId.HasValue)
                query = query.Where(cc => cc.TermID == activeTermId.Value);
            else
                return Ok(new List<object>()); // No terms exist at all

            if (User.IsInRole("Teacher"))
            {
                var teacherId = await GetTeacherIdFromTokenAsync();
                if (teacherId == null) return Forbid();
                query = query.Where(cc => cc.TeacherID == teacherId.Value);
            }

            var classes = await query
                .OrderBy(cc => cc.Class.ClassName)
                .Select(cc => new
                {
                    cc.CurrentClassID,
                    cc.TermID,
                    TermName    = cc.Term != null ? cc.Term.TermName : null,
                    ClassName   = cc.Class.ClassName,
                    TeacherName = cc.Teacher.TeacherName,
                    Section     = cc.Section != null ? cc.Section.Name : null,
                })
                .ToListAsync();

            return Ok(classes);
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
                IsMarked = existing.Count > 0,
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

            // Teachers must resolve to a teacher record; Admins may save without one (marker left null).
            var teacherId = await GetTeacherIdFromTokenAsync();
            if (User.IsInRole("Teacher") && teacherId == null) return Forbid();

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
                    row.MarkedByTeacherID = teacherId;
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
                        MarkedByTeacherID = teacherId,
                        CreatedOn = DateTime.UtcNow
                    });
                }
            }

            await dbContext.SaveChangesAsync();
            return Ok("Attendance saved.");
        }

        [HttpGet("student/{studentId:guid}/calendar")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetStudentCalendar([FromRoute] Guid studentId, [FromQuery] int? year)
        {
            if (studentId == Guid.Empty) return BadRequest("studentId is required.");

            var selectedYear = year ?? DateTime.UtcNow.Year;
            var start = new DateTime(selectedYear, 1, 1);
            var end = new DateTime(selectedYear, 12, 31);

            var entries = await dbContext.StudentAttendances
                .AsNoTracking()
                .Where(a => a.StudentID == studentId && a.AttendanceDate >= start && a.AttendanceDate <= end)
                .Select(a => new { a.AttendanceDate, a.Status })
                .ToListAsync();

            var lookup = entries.ToDictionary(
                x => (Month: x.AttendanceDate.Month, Day: x.AttendanceDate.Day),
                x => x.Status);

            static string? MapStatus(AttendanceStatus status) => status switch
            {
                AttendanceStatus.Present => "P",
                AttendanceStatus.Absent => "A",
                AttendanceStatus.Leave => "H",
                AttendanceStatus.Late => "L",
                _ => string.Empty
            };

            var months = Enumerable.Range(1, 12)
                .Select(m => new StudentAttendanceMonthDto
                {
                    Month = m,
                    Days = Enumerable.Range(1, 30)
                        .Select(d => lookup.TryGetValue((m, d), out var status) ? MapStatus(status) : string.Empty)
                        .ToList()
                })
                .ToList();

            var dto = new StudentAttendanceCalendarDto
            {
                Year = selectedYear,
                Months = months
            };

            return Ok(dto);
        }
    }
}
