using InstituteWebAPI.Models.DTO.Login;
using InstituteWebAPI.Models.DTO.Register;
using InstituteWebAPI.Repositories.IRepository;
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


        [HttpPost]
        [Route("Register")]


        //post /api/Auth/Register

        public async Task<ActionResult> Register([FromBody] RegisterDto registerDto)
        {

            var identityUser = new IdentityUser
            {UserName = registerDto.Username,
            Email = registerDto.Username

            };

          var identityResult =   await userManager.CreateAsync(identityUser, registerDto.Password);

            if (identityResult.Succeeded)
            {
                //add roles to this user
                if (registerDto.Roles != null && registerDto.Roles.Any())
                {

                    await userManager.AddToRolesAsync(identityUser, registerDto.Roles);

                    if (identityResult.Succeeded)
                    {
                        return Ok("User Registered Successfully. Please login");
                    }


                }
            }

            return BadRequest("Something went wrong.");

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
