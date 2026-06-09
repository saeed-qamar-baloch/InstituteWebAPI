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
    public class AdminGradeCriteriaController : ControllerBase
    {
        private readonly RozhnInstituteDbContext db;
        public AdminGradeCriteriaController(RozhnInstituteDbContext db) => this.db = db;

        // GET all grades sorted by MinPercentage descending (A first)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rows = await db.GradeCriterias
                .AsNoTracking()
                .OrderBy(g => g.DisplayOrder)
                .ToListAsync();
            return Ok(rows);
        }

        // PUT — bulk replace all grade criteria
        [HttpPut]
        public async Task<IActionResult> BulkUpdate([FromBody] List<UpsertGradeDto> dtos)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dtos == null || dtos.Count == 0) return BadRequest("At least one grade criterion is required.");

            var existing = await db.GradeCriterias.ToListAsync();
            db.GradeCriterias.RemoveRange(existing);

            int order = 1;
            foreach (var dto in dtos.OrderByDescending(d => d.MinPercentage))
            {
                db.GradeCriterias.Add(new GradeCriteria
                {
                    GradeCriteriaID = dto.GradeCriteriaID == Guid.Empty ? Guid.NewGuid() : dto.GradeCriteriaID,
                    GradeLabel      = dto.GradeLabel.Trim(),
                    MinPercentage   = dto.MinPercentage,
                    Description     = dto.Description?.Trim(),
                    DisplayOrder    = order++,
                });
            }

            await db.SaveChangesAsync();
            return Ok(await db.GradeCriterias.AsNoTracking().OrderBy(g => g.DisplayOrder).ToListAsync());
        }
    }

    public class UpsertGradeDto
    {
        public Guid GradeCriteriaID { get; set; }

        [Required, MaxLength(10)]
        public string GradeLabel { get; set; } = "";

        [Range(0, 100)]
        public float MinPercentage { get; set; }

        [MaxLength(50)]
        public string? Description { get; set; }
    }
}
