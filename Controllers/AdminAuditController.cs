using InstituteWebAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminAuditController : ControllerBase
    {
        private readonly RozhnInstituteDbContext dbContext;
        public AdminAuditController(RozhnInstituteDbContext dbContext) { this.dbContext = dbContext; }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? module,
            [FromQuery] string? user,
            [FromQuery] string? search,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int take = 300)
        {
            var q = dbContext.AuditLogs.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(module)) q = q.Where(a => a.Module == module);
            if (!string.IsNullOrWhiteSpace(user))   q = q.Where(a => a.UserName != null && a.UserName.Contains(user));
            if (from.HasValue) q = q.Where(a => a.CreatedOn >= from.Value);
            if (to.HasValue)   q = q.Where(a => a.CreatedOn <= to.Value.Date.AddDays(1));
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(a => a.Action.Contains(s) || (a.Details != null && a.Details.Contains(s)));
            }

            var rows = await q
                .OrderByDescending(a => a.CreatedOn)
                .Take(Math.Clamp(take, 1, 2000))
                .Select(a => new
                {
                    a.AuditLogID, a.UserName, a.Role, a.Module, a.Action,
                    a.Details, a.EntityType, a.EntityId, a.CreatedOn
                })
                .ToListAsync();

            return Ok(rows);
        }

        [HttpGet("modules")]
        public async Task<IActionResult> GetModules()
        {
            var mods = await dbContext.AuditLogs.AsNoTracking()
                .Select(a => a.Module).Distinct().OrderBy(m => m).ToListAsync();
            return Ok(mods);
        }
    }
}
