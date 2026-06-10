using InstituteWebAPI.Models.DTO.Login;
using InstituteWebAPI.Models.DTO.Register;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "Admin",
            "Teacher"
        };

        private readonly UserManager<IdentityUser> userManager;
        private readonly ITokenRepository tokenRepository;
        private readonly ITeacherRepository teacherRepository;
        private readonly ITeacherIdentityLinkRepository teacherIdentityLinkRepository;

        public AuthController(
            UserManager<IdentityUser> userManager,
            ITokenRepository tokenRepository,
            ITeacherRepository teacherRepository,
            ITeacherIdentityLinkRepository teacherIdentityLinkRepository)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
            this.teacherRepository = teacherRepository;
            this.teacherIdentityLinkRepository = teacherIdentityLinkRepository;
        }

        // POST /api/Auth/Register
        // Only Admin should create users in the system
        [HttpPost]
        [Route("Register")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto == null)
            {
                return BadRequest("Invalid data.");
            }

            if (string.IsNullOrEmpty(registerDto.Username) || string.IsNullOrEmpty(registerDto.Password))
            {
                return BadRequest("Username and Password are required.");
            }

            if (registerDto.Roles != null && registerDto.Roles.Any(r => !AllowedRoles.Contains(r)))
            {
                return BadRequest("Only Admin and Teacher roles are supported.");
            }

            var identityUser = new IdentityUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Username
            };

            var identityResult = await userManager.CreateAsync(identityUser, registerDto.Password);

            if (identityResult.Succeeded)
            {
                if (registerDto.Roles != null && registerDto.Roles.Any())
                {
                    var rolesToAssign = registerDto.Roles.Where(r => AllowedRoles.Contains(r)).ToArray();
                    var addRolesResult = await userManager.AddToRolesAsync(identityUser, rolesToAssign);
                    if (!addRolesResult.Succeeded)
                    {
                        return BadRequest("Failed to assign roles.");
                    }
                }

                return Ok("User Registered Successfully. Please login");
            }

            return BadRequest($"Something went wrong: {string.Join(", ", identityResult.Errors.Select(e => e.Description))}");
        }

        // POST /api/Auth/RegisterTeacher
        // Admin creates credentials for an existing teacher.
        // Links Teachers.RegistrationNo to IdentityUser.Id (user id) so teacher ownership checks work.
        [HttpPost]
        [Route("RegisterTeacher")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterTeacher([FromBody] RegisterTeacherCredentialDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var teacher = await teacherRepository.GetByIdAsync(dto.TeacherID);
            if (teacher == null)
            {
                return NotFound("Teacher not found.");
            }

            if (!string.IsNullOrWhiteSpace(teacher.IdentityUserId))
            {
                return BadRequest("This teacher already has a login account.");
            }

            var existingUser = await userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return BadRequest("A user with this email already exists.");
            }

            var identityUser = new IdentityUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var createResult = await userManager.CreateAsync(identityUser, dto.Password);
            if (!createResult.Succeeded)
            {
                return BadRequest(string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }

            var addRoleResult = await userManager.AddToRoleAsync(identityUser, "Teacher");
            if (!addRoleResult.Succeeded)
            {
                return BadRequest("Failed to assign Teacher role.");
            }

            // Link teacher with identity user id for authorization scoping
            await teacherIdentityLinkRepository.LinkTeacherToUserIdAsync(teacher.TeacherID, identityUser.Id);

            return Ok("Teacher credentials created successfully.");
        }

        // GET /api/Auth/teacher-account/{teacherId}
        // Returns whether the teacher already has an identity account and their email if so.
        [HttpGet]
        [Route("teacher-account/{teacherId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTeacherAccount(Guid teacherId)
        {
            var teacher = await teacherRepository.GetByIdAsync(teacherId);
            if (teacher == null) return NotFound("Teacher not found.");

            // The login account link is stored in teacher.IdentityUserId.
            IdentityUser? identityUser = null;
            if (!string.IsNullOrWhiteSpace(teacher.IdentityUserId))
                identityUser = await userManager.FindByIdAsync(teacher.IdentityUserId);
            // Legacy fallback: older records kept the user id in RegistrationNo.
            if (identityUser == null && !string.IsNullOrWhiteSpace(teacher.RegistrationNo))
                identityUser = await userManager.FindByIdAsync(teacher.RegistrationNo);

            if (identityUser == null)
                return Ok(new { hasAccount = false, email = (string?)null });

            return Ok(new { hasAccount = true, email = identityUser.Email });
        }

        // POST /api/Auth/reset-teacher-password
        // Admin resets a teacher's login password.
        [HttpPost]
        [Route("reset-teacher-password")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetTeacherPassword([FromBody] ResetTeacherPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var teacher = await teacherRepository.GetByIdAsync(dto.TeacherID);
            if (teacher == null) return NotFound("Teacher not found.");

            IdentityUser? identityUser = null;
            if (!string.IsNullOrWhiteSpace(teacher.IdentityUserId))
                identityUser = await userManager.FindByIdAsync(teacher.IdentityUserId);
            if (identityUser == null && !string.IsNullOrWhiteSpace(teacher.RegistrationNo))
                identityUser = await userManager.FindByIdAsync(teacher.RegistrationNo);

            if (identityUser == null)
                return BadRequest("No account found for this teacher. Create one first.");

            var token = await userManager.GeneratePasswordResetTokenAsync(identityUser);
            var result = await userManager.ResetPasswordAsync(identityUser, token, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

            return Ok("Password reset successfully.");
        }

        [HttpPost]
        [Route("Login")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await userManager.FindByEmailAsync(loginDto.Username);
            if (user != null)
            {
                var checkPasswordResult = await userManager.CheckPasswordAsync(user, loginDto.Password);

                if (checkPasswordResult)
                {
                    // get roles
                    var roles = await userManager.GetRolesAsync(user);

                    if (roles != null)
                    {
                        var jwtToken = tokenRepository.CreateJWTToken(user, roles.ToList());

                        var response = new LoginResponseDto
                        {
                            JwtToken = jwtToken
                        };

                        return Ok(response);
                    }
                }
            }

            return BadRequest("Username or password incorrect.");
        }
    }
}
