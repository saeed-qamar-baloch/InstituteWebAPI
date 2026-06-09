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
    public class AdminTeacherSalaryController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;

        public AdminTeacherSalaryController(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // ── DTOs ──────────────────────────────────────────────────────────────
        public class TeacherSalaryDto
        {
            public Guid TeacherSalaryID { get; set; }
            public Guid TeacherID { get; set; }
            public string? TeacherName { get; set; }
            public string? RegistrationNo { get; set; }
            public string? Picture { get; set; }
            public int SalaryMonth { get; set; }
            public int SalaryYear { get; set; }
            public decimal Amount { get; set; }
            public DateTime PaidOn { get; set; }
            public string? Notes { get; set; }
            public DateTime CreatedOn { get; set; }
        }

        public class AddTeacherSalaryDto
        {
            [Required]
            public Guid TeacherID { get; set; }

            [Range(1, 12)]
            public int SalaryMonth { get; set; }

            [Range(2000, 3000)]
            public int SalaryYear { get; set; }

            [Range(0, double.MaxValue)]
            public decimal Amount { get; set; }

            [Required]
            public DateTime PaidOn { get; set; }

            public string? Notes { get; set; }
        }

        public class ActiveTeacherDto
        {
            public Guid TeacherID { get; set; }
            public string? TeacherName { get; set; }
            public string? RegistrationNo { get; set; }
            public string? Picture { get; set; }
        }

        // ── GET active teachers (for the salary entry dropdown) ───────────────
        [HttpGet("active-teachers")]
        public async Task<IActionResult> GetActiveTeachers()
        {
            var teachers = await dbContext.Teachers
                .AsNoTracking()
                .Where(t => t.IsTeaching)
                .OrderBy(t => t.TeacherName)
                .Select(t => new ActiveTeacherDto
                {
                    TeacherID      = t.TeacherID,
                    TeacherName    = t.TeacherName,
                    RegistrationNo = t.RegistrationNo,
                    Picture        = t.Picture
                })
                .ToListAsync();

            return Ok(teachers);
        }

        // ── GET salary records (optionally filtered) ──────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? teacherId,
            [FromQuery] int? year,
            [FromQuery] int? month)
        {
            var query = dbContext.TeacherSalaries
                .AsNoTracking()
                .Include(s => s.Teacher)
                .AsQueryable();

            if (teacherId.HasValue) query = query.Where(s => s.TeacherID == teacherId.Value);
            if (year.HasValue)      query = query.Where(s => s.SalaryYear == year.Value);
            if (month.HasValue)     query = query.Where(s => s.SalaryMonth == month.Value);

            var rows = await query
                .OrderByDescending(s => s.SalaryYear)
                .ThenByDescending(s => s.SalaryMonth)
                .ThenByDescending(s => s.PaidOn)
                .Select(s => new TeacherSalaryDto
                {
                    TeacherSalaryID = s.TeacherSalaryID,
                    TeacherID       = s.TeacherID,
                    TeacherName     = s.Teacher != null ? s.Teacher.TeacherName : null,
                    RegistrationNo  = s.Teacher != null ? s.Teacher.RegistrationNo : null,
                    Picture         = s.Teacher != null ? s.Teacher.Picture : null,
                    SalaryMonth     = s.SalaryMonth,
                    SalaryYear      = s.SalaryYear,
                    Amount          = s.Amount,
                    PaidOn          = s.PaidOn,
                    Notes           = s.Notes,
                    CreatedOn       = s.CreatedOn
                })
                .ToListAsync();

            return Ok(rows);
        }

        // ── POST create salary ────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddTeacherSalaryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var teacher = await dbContext.Teachers
                .FirstOrDefaultAsync(t => t.TeacherID == dto.TeacherID);
            if (teacher == null) return NotFound(new { message = "Teacher not found." });
            if (!teacher.IsTeaching)
                return BadRequest(new { message = "Salary can only be added for active teachers." });

            var duplicate = await dbContext.TeacherSalaries.AnyAsync(s =>
                s.TeacherID == dto.TeacherID &&
                s.SalaryYear == dto.SalaryYear &&
                s.SalaryMonth == dto.SalaryMonth);
            if (duplicate)
                return Conflict(new { message = "A salary for this teacher and month already exists." });

            var entity = new TeacherSalary
            {
                TeacherSalaryID = Guid.NewGuid(),
                TeacherID       = dto.TeacherID,
                SalaryMonth     = dto.SalaryMonth,
                SalaryYear      = dto.SalaryYear,
                Amount          = dto.Amount,
                PaidOn          = dto.PaidOn,
                Notes           = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
                CreatedOn       = DateTime.UtcNow
            };

            dbContext.TeacherSalaries.Add(entity);
            await dbContext.SaveChangesAsync();

            return Ok(new TeacherSalaryDto
            {
                TeacherSalaryID = entity.TeacherSalaryID,
                TeacherID       = entity.TeacherID,
                TeacherName     = teacher.TeacherName,
                RegistrationNo  = teacher.RegistrationNo,
                Picture         = teacher.Picture,
                SalaryMonth     = entity.SalaryMonth,
                SalaryYear      = entity.SalaryYear,
                Amount          = entity.Amount,
                PaidOn          = entity.PaidOn,
                Notes           = entity.Notes,
                CreatedOn       = entity.CreatedOn
            });
        }

        // ── DELETE salary ─────────────────────────────────────────────────────
        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await dbContext.TeacherSalaries.FirstOrDefaultAsync(s => s.TeacherSalaryID == id);
            if (existing == null) return NotFound();

            dbContext.TeacherSalaries.Remove(existing);
            await dbContext.SaveChangesAsync();
            return Ok(new { id });
        }
    }
}
