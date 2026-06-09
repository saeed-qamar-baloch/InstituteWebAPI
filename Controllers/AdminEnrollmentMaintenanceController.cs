using InstituteWebAPI.Data;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Controllers
{
    // Maintenance tools for keeping the student list's "Enrolled" flag in sync with
    // actual class enrolment in the current (active) term, plus diagnostics for
    // mismatches an admin should clean up.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminEnrollmentMaintenanceController : ControllerBase
    {
        private readonly RozhnInstituteDbContext db;

        public AdminEnrollmentMaintenanceController(RozhnInstituteDbContext db)
        {
            this.db = db;
        }

        private async Task<Term?> GetActiveTermAsync()
        {
            return await db.Term.AsNoTracking()
                       .Where(t => t.IsActive)
                       .OrderByDescending(t => t.TermStart)
                       .FirstOrDefaultAsync()
                   ?? await db.Term.AsNoTracking()
                       .OrderByDescending(t => t.TermStart)
                       .FirstOrDefaultAsync();
        }

        // Students with an "Enrolled" class enrolment in the active term.
        private async Task<List<Guid>> GetEnrolledInActiveTermAsync(Guid activeTermId)
        {
            return await db.ClassStudents.AsNoTracking()
                .Where(cs => cs.Status == "Enrolled" && cs.CurrentClass.TermID == activeTermId)
                .Select(cs => cs.StudentID)
                .Distinct()
                .ToListAsync();
        }

        public class StudentRefDto
        {
            public Guid StudentID { get; set; }
            public string? RegistrationNo { get; set; }
            public string? StudentName { get; set; }
            public string? FatherName { get; set; }
        }

        // ── POST /api/AdminEnrollmentMaintenance/sync-enrolled-status ──────────
        // Marks Students.IsEnrolled = true ONLY for students enrolled in a class in
        // the active term; everyone else is set to not-enrolled.
        [HttpPost("sync-enrolled-status")]
        public async Task<IActionResult> SyncEnrolledStatus()
        {
            var term = await GetActiveTermAsync();
            if (term == null)
                return BadRequest(new { message = "No term found. Create and activate a term first." });

            var enrolledIds = await GetEnrolledInActiveTermAsync(term.TermID);

            // Set Enrolled for students who have a class this term but aren't flagged.
            var markedEnrolled = await db.Students
                .Where(s => enrolledIds.Contains(s.StudentID) && !s.IsEnrolled)
                .ExecuteUpdateAsync(set => set.SetProperty(x => x.IsEnrolled, true));

            // Clear the flag for everyone without a class this term.
            var markedNotEnrolled = await db.Students
                .Where(s => !enrolledIds.Contains(s.StudentID) && s.IsEnrolled)
                .ExecuteUpdateAsync(set => set.SetProperty(x => x.IsEnrolled, false));

            return Ok(new
            {
                activeTerm = term.TermName,
                enrolledInActiveTerm = enrolledIds.Count,
                markedEnrolled,
                markedNotEnrolled,
                message = $"{enrolledIds.Count} student(s) enrolled in {term.TermName}. " +
                          $"{markedEnrolled} marked Enrolled, {markedNotEnrolled} cleared."
            });
        }

        // ── GET /api/AdminEnrollmentMaintenance/diagnostics ───────────────────
        // Surfaces students whose records are inconsistent:
        //  • Enrolled in the list but no active admission.
        //  • Active admission or Enrolled in the list but no class in the active term.
        [HttpGet("diagnostics")]
        public async Task<IActionResult> Diagnostics()
        {
            var term = await GetActiveTermAsync();
            if (term == null)
                return Ok(new
                {
                    activeTerm = (string?)null,
                    enrolledNoActiveAdmission = new List<StudentRefDto>(),
                    noClassAssigned = new List<StudentRefDto>(),
                    message = "No term found."
                });

            var enrolledIds = await GetEnrolledInActiveTermAsync(term.TermID);
            var activeAdmissionIds = await db.Admissions.AsNoTracking()
                .Where(a => a.IsActive)
                .Select(a => a.StudentID)
                .Distinct()
                .ToListAsync();

            // Enrolled in the list but no active admission on record.
            var enrolledNoActiveAdmission = await db.Students.AsNoTracking()
                .Where(s => s.IsEnrolled && !activeAdmissionIds.Contains(s.StudentID))
                .OrderBy(s => s.StudentName)
                .Select(s => new StudentRefDto
                {
                    StudentID = s.StudentID,
                    RegistrationNo = s.RegistrationNo,
                    StudentName = s.StudentName,
                    FatherName = s.FatherName,
                })
                .ToListAsync();

            // Active admission OR Enrolled in the list, but no class in the active term.
            var noClassAssigned = await db.Students.AsNoTracking()
                .Where(s => (s.IsEnrolled || activeAdmissionIds.Contains(s.StudentID))
                            && !enrolledIds.Contains(s.StudentID))
                .OrderBy(s => s.StudentName)
                .Select(s => new StudentRefDto
                {
                    StudentID = s.StudentID,
                    RegistrationNo = s.RegistrationNo,
                    StudentName = s.StudentName,
                    FatherName = s.FatherName,
                })
                .ToListAsync();

            return Ok(new
            {
                activeTerm = term.TermName,
                enrolledInActiveTerm = enrolledIds.Count,
                enrolledNoActiveAdmissionCount = enrolledNoActiveAdmission.Count,
                noClassAssignedCount = noClassAssigned.Count,
                enrolledNoActiveAdmission,
                noClassAssigned,
            });
        }
    }
}
