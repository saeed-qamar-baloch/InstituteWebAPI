using Google.Apis.Auth;
using InstituteWebAPI.Models.DTO.Learners;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    /// <summary>Self-service auth for "Learn English" learner accounts (public).</summary>
    [Route("api/learner/auth")]
    [ApiController]
    [AllowAnonymous]
    public class LearnerAuthController : ControllerBase
    {
        private readonly ILearnerRepository learners;
        private readonly IPasswordHasher<Learner> hasher;
        private readonly IConfiguration config;

        public LearnerAuthController(ILearnerRepository learners, IPasswordHasher<Learner> hasher, IConfiguration config)
        {
            this.learners = learners;
            this.hasher = hasher;
            this.config = config;
        }

        // POST api/learner/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterLearnerDto dto)
        {
            var existing = await learners.GetByEmailAsync(dto.Email);
            if (existing != null)
                return Conflict(new { message = "An account with this email already exists. Please log in." });

            var learner = new Learner { DisplayName = dto.DisplayName.Trim(), Email = dto.Email.Trim() };
            learner.PasswordHash = hasher.HashPassword(learner, dto.Password);
            learner = await learners.AddAsync(learner);

            return Ok(new { token = LearnerHelpers.CreateToken(learner, config), profile = LearnerHelpers.Profile(learner) });
        }

        // POST api/learner/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginLearnerDto dto)
        {
            var learner = await learners.GetByEmailAsync(dto.Email);
            if (learner?.PasswordHash == null)
                return Unauthorized(new { message = "Invalid email or password." });

            var result = hasher.VerifyHashedPassword(learner, learner.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(new { token = LearnerHelpers.CreateToken(learner, config), profile = LearnerHelpers.Profile(learner) });
        }

        // POST api/learner/auth/google
        [HttpPost("google")]
        public async Task<IActionResult> Google([FromBody] GoogleLoginDto dto)
        {
            var clientId = config["Google:ClientId"];
            if (string.IsNullOrWhiteSpace(clientId))
                return BadRequest(new { message = "Google sign-in is not configured on the server." });

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.Credential,
                    new GoogleJsonWebSignature.ValidationSettings { Audience = new[] { clientId } });
            }
            catch
            {
                return Unauthorized(new { message = "Could not verify the Google sign-in." });
            }

            var learner = await learners.GetByGoogleSubjectAsync(payload.Subject)
                          ?? (string.IsNullOrEmpty(payload.Email) ? null : await learners.GetByEmailAsync(payload.Email));

            if (learner == null)
            {
                learner = new Learner
                {
                    DisplayName = string.IsNullOrWhiteSpace(payload.Name) ? (payload.Email ?? "Learner") : payload.Name,
                    Email = payload.Email,
                    GoogleSubject = payload.Subject,
                };
                learner = await learners.AddAsync(learner);
            }
            else if (string.IsNullOrEmpty(learner.GoogleSubject))
            {
                learner.GoogleSubject = payload.Subject; // link Google to an existing email account
                await learners.UpdateAsync(learner);
            }

            return Ok(new { token = LearnerHelpers.CreateToken(learner, config), profile = LearnerHelpers.Profile(learner) });
        }
    }
}
