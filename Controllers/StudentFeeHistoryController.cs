using InstituteWebAPI.Data;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class StudentFeeHistoryController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;

        public StudentFeeHistoryController(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // ── DTOs ─────────────────────────────────────────────────────────────

        public class AddFeeHistoryDto
        {
            public Guid AdmissionID   { get; set; }
            public Guid CourseID      { get; set; }
            public decimal FeeAmount  { get; set; }
            /// <summary>First day of the month this fee becomes effective.</summary>
            public DateTime EffectiveFrom { get; set; }
            public string? Remarks    { get; set; }
        }

        public class UpdateFeeHistoryDto
        {
            public decimal FeeAmount  { get; set; }
            public DateTime EffectiveFrom { get; set; }
            public DateTime? EffectiveTo  { get; set; }
            public bool IsActive      { get; set; }
            public string? Remarks    { get; set; }
        }

        private static object Shape(StudentFeeHistory h) => new
        {
            h.FeeHistoryID,
            h.AdmissionID,
            h.CourseID,
            CourseName    = h.Course?.CourseName,
            h.FeeAmount,
            h.EffectiveFrom,
            h.EffectiveTo,
            h.IsActive,
            h.Remarks,
            h.CreatedAt
        };

        // ── GET api/StudentFeeHistory?admissionId= ────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? admissionId)
        {
            var query = dbContext.StudentFeeHistories
                .AsNoTracking()
                .Include(h => h.Course)
                .AsQueryable();

            if (admissionId.HasValue)
                query = query.Where(h => h.AdmissionID == admissionId.Value);

            var list = await query
                .OrderByDescending(h => h.EffectiveFrom)
                .ToListAsync();

            return Ok(list.Select(Shape));
        }

        // ── GET api/StudentFeeHistory/{id} ────────────────────────────────────
        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var h = await dbContext.StudentFeeHistories
                .AsNoTracking()
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x => x.FeeHistoryID == id);

            if (h == null) return NotFound();
            return Ok(Shape(h));
        }

        // ── GET api/StudentFeeHistory/by-admission/{admissionId} ─────────────
        // Convenience: returns all history rows for an admission, ordered oldest→newest.
        [HttpGet("by-admission/{admissionId:Guid}")]
        public async Task<IActionResult> GetByAdmission([FromRoute] Guid admissionId)
        {
            var list = await dbContext.StudentFeeHistories
                .AsNoTracking()
                .Include(h => h.Course)
                .Where(h => h.AdmissionID == admissionId)
                .OrderBy(h => h.EffectiveFrom)
                .ToListAsync();

            return Ok(list.Select(Shape));
        }

        // ── POST api/StudentFeeHistory ────────────────────────────────────────
        // Adding a new fee history row automatically:
        //   • closes the previous active row's EffectiveTo to one day before EffectiveFrom
        //   • marks all other rows IsActive = false
        //   • marks the new row IsActive = true
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddFeeHistoryDto dto)
        {
            if (dto.AdmissionID == Guid.Empty) return BadRequest("AdmissionID is required.");
            if (dto.CourseID    == Guid.Empty) return BadRequest("CourseID is required.");
            if (dto.FeeAmount   < 0)           return BadRequest("FeeAmount must be >= 0.");

            var admissionExists = await dbContext.Admissions
                .AsNoTracking()
                .AnyAsync(a => a.AdmissionID == dto.AdmissionID);
            if (!admissionExists) return NotFound("Admission not found.");

            var courseExists = await dbContext.Courses
                .AsNoTracking()
                .AnyAsync(c => c.CourseID == dto.CourseID);
            if (!courseExists) return NotFound("Course not found.");

            var effectiveFrom = new DateTime(dto.EffectiveFrom.Year, dto.EffectiveFrom.Month, 1);

            // Close existing active row
            var previous = await dbContext.StudentFeeHistories
                .Where(h => h.AdmissionID == dto.AdmissionID && h.IsActive)
                .ToListAsync();

            foreach (var prev in previous)
            {
                prev.IsActive    = false;
                prev.EffectiveTo = effectiveFrom.AddDays(-1);
            }

            var newRow = new StudentFeeHistory
            {
                FeeHistoryID  = Guid.NewGuid(),
                AdmissionID   = dto.AdmissionID,
                CourseID      = dto.CourseID,
                FeeAmount     = dto.FeeAmount,
                EffectiveFrom = effectiveFrom,
                EffectiveTo   = null,
                IsActive      = true,
                Remarks       = dto.Remarks,
                CreatedAt     = DateTime.UtcNow
            };

            // Also update Admission.MonthlyFee to stay in sync
            var admission = await dbContext.Admissions
                .FirstOrDefaultAsync(a => a.AdmissionID == dto.AdmissionID);
            if (admission != null)
            {
                admission.MonthlyFee  = dto.FeeAmount;
                admission.ModifiedAt  = DateTime.UtcNow;
            }

            dbContext.StudentFeeHistories.Add(newRow);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = newRow.FeeHistoryID }, Shape(newRow));
        }

        // ── PUT api/StudentFeeHistory/{id} ────────────────────────────────────
        // Manual correction of a history row (e.g. wrong amount was entered).
        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateFeeHistoryDto dto)
        {
            if (dto.FeeAmount < 0) return BadRequest("FeeAmount must be >= 0.");

            var row = await dbContext.StudentFeeHistories.FindAsync(id);
            if (row == null) return NotFound();

            row.FeeAmount     = dto.FeeAmount;
            row.EffectiveFrom = new DateTime(dto.EffectiveFrom.Year, dto.EffectiveFrom.Month, 1);
            row.EffectiveTo   = dto.EffectiveTo.HasValue
                ? new DateTime(dto.EffectiveTo.Value.Year, dto.EffectiveTo.Value.Month, 1)
                : null;
            row.IsActive  = dto.IsActive;
            row.Remarks   = dto.Remarks;

            // If marking this row active, sync Admission.MonthlyFee
            if (dto.IsActive)
            {
                var admission = await dbContext.Admissions
                    .FirstOrDefaultAsync(a => a.AdmissionID == row.AdmissionID);
                if (admission != null)
                {
                    admission.MonthlyFee = dto.FeeAmount;
                    admission.ModifiedAt = DateTime.UtcNow;
                }
            }

            await dbContext.SaveChangesAsync();
            return Ok(Shape(row));
        }

        // ── DELETE api/StudentFeeHistory/{id} ─────────────────────────────────
        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var row = await dbContext.StudentFeeHistories.FindAsync(id);
            if (row == null) return NotFound();

            if (row.IsActive)
                return BadRequest("Cannot delete the active fee history row. Set an earlier row as active first.");

            dbContext.StudentFeeHistories.Remove(row);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
