using InstituteWebAPI.Models.DTO.Login;
using InstituteWebAPI.Models.DTO.Register;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly ITokenRepository tokenRepository;

        public AuthController(UserManager<IdentityUser> userManager, ITokenRepository tokenRepository )
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
        }

        //post /api/Auth/Register

        [HttpPost]
        [Route("Register")]
        [Authorize(Roles ="Teacher")]
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
                    var addRolesResult = await userManager.AddToRolesAsync(identityUser, registerDto.Roles);
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
              var checkPasswordResult=   await userManager.CheckPasswordAsync(user, loginDto.Password);

                if(checkPasswordResult)
                {
                    //get roles
                    var roles = await userManager.GetRolesAsync(user);

                    if (roles != null)
                    {
                     var jwtToken=   tokenRepository.CreateJWTToken(user, roles.ToList());

                        var response = new LoginResponseDto
                        {
                            JwtToken = jwtToken
                        };

                        return Ok(response);
                    }
                    //token
                    

                    
                }

            }

            return BadRequest("Username or password incorrect.");

        }


    }
}
