using InstituteWebAPI.Data;
using InstituteWebAPI.Services.Access;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminRolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RozhnInstituteDbContext dbContext;

        public AdminRolesController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, RozhnInstituteDbContext dbContext)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.dbContext = dbContext;
        }

        // Roles that cannot be deleted/renamed; Admin is always full-access.
        private static readonly HashSet<string> SystemRoles = new(StringComparer.OrdinalIgnoreCase) { "Admin", "Teacher" };

        public class CreateRoleDto { [Required] public string Name { get; set; } = ""; }
        public class PermissionsDto { public List<string> Areas { get; set; } = new(); }

        [HttpGet("areas")]
        public IActionResult GetAreas() => Ok(AccessAreas.All.Select(a => new { a.Key, a.Label }));

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await roleManager.Roles.AsNoTracking().Select(r => r.Name!).ToListAsync();
            var perms = await dbContext.RolePermissions.AsNoTracking().ToListAsync();
            var allUsers = await userManager.Users.AsNoTracking().ToListAsync();

            var rows = new List<object>();
            foreach (var role in roles.OrderBy(r => r))
            {
                var isAdmin = role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
                var areas = isAdmin
                    ? AccessAreas.All.Select(a => a.Key).ToList()
                    : perms.Where(p => p.RoleName == role).Select(p => p.Area).ToList();

                // count users in this role
                var count = 0;
                foreach (var u in allUsers)
                    if (await userManager.IsInRoleAsync(u, role)) count++;

                rows.Add(new
                {
                    Name = role,
                    Areas = areas,
                    UserCount = count,
                    Locked = isAdmin,        // Admin = full, cannot edit/delete
                    System = SystemRoles.Contains(role),
                });
            }
            return Ok(rows);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
        {
            var name = (dto.Name ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Role name is required." });
            if (await roleManager.RoleExistsAsync(name)) return BadRequest(new { message = "Role already exists." });

            var res = await roleManager.CreateAsync(new IdentityRole(name));
            if (!res.Succeeded) return BadRequest(new { message = string.Join(" ", res.Errors.Select(e => e.Description)) });
            return Ok(new { name });
        }

        [HttpPut("{name}/permissions")]
        public async Task<IActionResult> SetPermissions(string name, [FromBody] PermissionsDto dto)
        {
            if (name.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Admin has full access and cannot be modified." });
            if (!await roleManager.RoleExistsAsync(name)) return NotFound();

            var areas = (dto.Areas ?? new()).Where(a => AccessAreas.Keys.Contains(a)).Distinct().ToList();

            var existing = dbContext.RolePermissions.Where(p => p.RoleName == name);
            dbContext.RolePermissions.RemoveRange(existing);
            foreach (var a in areas)
                dbContext.RolePermissions.Add(new RolePermission { RolePermissionID = Guid.NewGuid(), RoleName = name, Area = a });
            await dbContext.SaveChangesAsync();

            return Ok(new { name, areas });
        }

        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            if (SystemRoles.Contains(name))
                return BadRequest(new { message = "Built-in roles cannot be deleted." });
            var role = await roleManager.FindByNameAsync(name);
            if (role == null) return NotFound();

            // Block delete if users are assigned
            var users = await userManager.GetUsersInRoleAsync(name);
            if (users.Count > 0) return BadRequest(new { message = $"{users.Count} user(s) still have this role." });

            dbContext.RolePermissions.RemoveRange(dbContext.RolePermissions.Where(p => p.RoleName == name));
            await dbContext.SaveChangesAsync();
            await roleManager.DeleteAsync(role);
            return Ok(new { name });
        }
    }
}
