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
    public class AdminExpenseCategoryController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        public AdminExpenseCategoryController(RozhnInstituteDbContext dbContext) { this.dbContext = dbContext; }

        public class CategoryDto
        {
            public Guid ExpenseCategoryID { get; set; }
            public string Name { get; set; } = "";
            public string? Description { get; set; }
            public bool IsActive { get; set; }
            public int ExpenseCount { get; set; }
        }

        public class SaveCategoryDto
        {
            [Required] public string Name { get; set; } = "";
            public string? Description { get; set; }
            public bool IsActive { get; set; } = true;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rows = await dbContext.ExpenseCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    ExpenseCategoryID = c.ExpenseCategoryID,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    ExpenseCount = c.Expenses.Count,
                })
                .ToListAsync();
            return Ok(rows);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = new ExpenseCategory
            {
                ExpenseCategoryID = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                IsActive = dto.IsActive,
                CreatedOn = DateTime.UtcNow,
            };
            dbContext.ExpenseCategories.Add(entity);
            await dbContext.SaveChangesAsync();
            return Ok(new { entity.ExpenseCategoryID });
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SaveCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = await dbContext.ExpenseCategories.FirstOrDefaultAsync(c => c.ExpenseCategoryID == id);
            if (entity == null) return NotFound();
            entity.Name = dto.Name.Trim();
            entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            entity.IsActive = dto.IsActive;
            await dbContext.SaveChangesAsync();
            return Ok(new { entity.ExpenseCategoryID });
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await dbContext.ExpenseCategories.Include(c => c.Expenses)
                .FirstOrDefaultAsync(c => c.ExpenseCategoryID == id);
            if (entity == null) return NotFound();
            if (entity.Expenses.Count > 0)
                return BadRequest(new { message = "Cannot delete a category that has expenses. Deactivate it instead." });
            dbContext.ExpenseCategories.Remove(entity);
            await dbContext.SaveChangesAsync();
            return Ok(new { id });
        }
    }
}
