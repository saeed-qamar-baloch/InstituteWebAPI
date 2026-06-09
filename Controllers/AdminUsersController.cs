using InstituteWebAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly RozhnInstituteDbContext dbContext;

        public AdminUsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, RozhnInstituteDbContext dbContext)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.dbContext = dbContext;
        }

        public class CreateUserDto
        {
            [Required] public string Email { get; set; } = "";
            [Required, MinLength(6)] public string Password { get; set; } = "";
            public string Role { get; set; } = "Teacher";
        }
        public class SetRoleDto { public string Role { get; set; } = "Teacher"; }
        public class ResetPasswordDto { [Required, MinLength(6)] public string NewPassword { get; set; } = ""; }

        private string? CurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await roleManager.Roles.Select(r => r.Name!).OrderBy(n => n).ToListAsync();
            return Ok(roles);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var users = await userManager.Users.AsNoTracking().ToListAsync();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                users = users.Where(u => (u.Email ?? "").Contains(s, StringComparison.OrdinalIgnoreCase)
                                      || (u.UserName ?? "").Contains(s, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Map linked teacher names
            var linkedTeachers = await dbContext.Teachers.AsNoTracking()
                .Where(t => t.IdentityUserId != null)
                .Select(t => new { t.IdentityUserId, t.TeacherName })
                .ToListAsync();
            var teacherByUser = linkedTeachers.ToDictionary(x => x.IdentityUserId!, x => x.TeacherName);

            var now = DateTimeOffset.UtcNow;
            var rows = new List<object>();
            foreach (var u in users.OrderBy(u => u.Email))
            {
                var roles = await userManager.GetRolesAsync(u);
                var disabled = u.LockoutEnd.HasValue && u.LockoutEnd.Value > now;
                rows.Add(new
                {
                    u.Id,
                    u.Email,
                    Username = u.UserName,
                    Roles = roles,
                    Disabled = disabled,
                    TeacherName = teacherByUser.TryGetValue(u.Id, out var tn) ? tn : null,
                    IsSelf = u.Id == CurrentUserId(),
                });
            }
            return Ok(rows);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var role = (dto.Role ?? "Teacher").Trim();

            if (await userManager.FindByEmailAsync(dto.Email) != null)
                return BadRequest(new { message = "A user with this email already exists." });

            var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
            var create = await userManager.CreateAsync(user, dto.Password);
            if (!create.Succeeded)
                return BadRequest(new { message = string.Join(" ", create.Errors.Select(e => e.Description)) });

            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
            await userManager.AddToRoleAsync(user, role);

            return Ok(new { user.Id });
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> SetRole(string id, [FromBody] SetRoleDto dto)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var role = (dto.Role ?? "").Trim();
            if (string.IsNullOrWhiteSpace(role) || !await roleManager.RoleExistsAsync(role))
                return BadRequest(new { message = "Invalid role." });

            if (id == CurrentUserId() && !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "You cannot change your own Admin role." });

            var current = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, current);
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
            await userManager.AddToRoleAsync(user, role);
            return Ok(new { id, role });
        }

        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var res = await userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!res.Succeeded)
                return BadRequest(new { message = string.Join(" ", res.Errors.Select(e => e.Description)) });
            return Ok(new { message = "Password reset." });
        }

        [HttpPost("{id}/toggle-enable")]
        public async Task<IActionResult> ToggleEnable(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (id == CurrentUserId()) return BadRequest(new { message = "You cannot disable your own account." });

            var now = DateTimeOffset.UtcNow;
            var disabled = user.LockoutEnd.HasValue && user.LockoutEnd.Value > now;
            if (disabled)
            {
                await userManager.SetLockoutEndDateAsync(user, null);
                return Ok(new { id, disabled = false });
            }
            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            return Ok(new { id, disabled = true });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == CurrentUserId()) return BadRequest(new { message = "You cannot delete your own account." });
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Unlink any teacher tied to this login so they aren't orphaned
            var teachers = await dbContext.Teachers.Where(t => t.IdentityUserId == id).ToListAsync();
            foreach (var t in teachers) t.IdentityUserId = null;
            if (teachers.Count > 0) await dbContext.SaveChangesAsync();

            var res = await userManager.DeleteAsync(user);
            if (!res.Succeeded)
                return BadRequest(new { message = string.Join(" ", res.Errors.Select(e => e.Description)) });
            return Ok(new { id });
        }
    }
}
