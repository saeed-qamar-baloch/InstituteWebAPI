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
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RozhnInstituteDbContext dbContext;

        public AccountController(UserManager<IdentityUser> userManager, RozhnInstituteDbContext dbContext)
        {
            this.userManager = userManager;
            this.dbContext = dbContext;
        }

        public class ChangePasswordDto
        {
            [Required] public string CurrentPassword { get; set; } = "";
            [Required, MinLength(6)] public string NewPassword { get; set; } = "";
        }

        // ── GET api/Account/me ────────────────────────────────────────────────
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var roles = await userManager.GetRolesAsync(user);

            // If this account is linked to a teacher, surface their display name.
            string? teacherName = null;
            var teacher = await dbContext.Teachers.AsNoTracking()
                .FirstOrDefaultAsync(t => t.IdentityUserId == user.Id);
            if (teacher != null) teacherName = teacher.TeacherName;

            return Ok(new
            {
                user.Id,
                Username = user.UserName,
                user.Email,
                user.PhoneNumber,
                Roles = roles,
                TeacherName = teacherName,
            });
        }

        // ── GET api/Account/permissions ──────────────────────────────────────
        // Areas the current user may access (union across roles). Admin = all.
        [HttpGet("permissions")]
        public async Task<IActionResult> Permissions()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var roles = await userManager.GetRolesAsync(user);
            if (roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
                return Ok(new { areas = InstituteWebAPI.Services.Access.AccessAreas.All.Select(a => a.Key), isAdmin = true });

            var areas = await dbContext.RolePermissions.AsNoTracking()
                .Where(p => roles.Contains(p.RoleName))
                .Select(p => p.Area)
                .Distinct()
                .ToListAsync();

            return Ok(new { areas, isAdmin = false });
        }

        // ── POST api/Account/change-password ─────────────────────────────────
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.CurrentPassword == dto.NewPassword)
                return BadRequest(new { message = "New password must be different from the current one." });

            var user = await userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(new { message = string.Join(" ", result.Errors.Select(e => e.Description)) });

            return Ok(new { message = "Password changed successfully." });
        }
    }
}
