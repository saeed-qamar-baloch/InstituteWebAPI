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

        public AuthController(UserManager<IdentityUser> userManager, ITokenRepository tokenRepository)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
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

        [HttpPost]
        [Route("Login")]
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
