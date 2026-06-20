using System.Security.Claims;
using InstituteWebAPI.Models.DTO.Learners;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebAPI.Repositories.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    /// <summary>Learner self-service: profile, progress &amp; streak. Learner role only.</summary>
    [Route("api/learner")]
    [ApiController]
    [Authorize(Roles = "Learner")]
    public class LearnerController : ControllerBase
    {
        private readonly ILearnerRepository learners;
        public LearnerController(ILearnerRepository learners) { this.learners = learners; }

        private Guid? CurrentId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }

        // GET api/learner/me
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var id = CurrentId();
            if (id == null) return Unauthorized();
            var learner = await learners.GetByIdAsync(id.Value);
            if (learner == null) return NotFound();
            return Ok(LearnerHelpers.Profile(learner));
        }

        // POST api/learner/sync — union the client's progress with the stored
        // progress, record today as a learning day, recompute the streak, save,
        // and return the merged canonical profile.
        [HttpPost("sync")]
        public async Task<IActionResult> Sync([FromBody] SyncProgressDto dto)
        {
            var id = CurrentId();
            if (id == null) return Unauthorized();
            var learner = await learners.GetByIdAsync(id.Value);
            if (learner == null) return NotFound();

            // merge completed lessons (union)
            var completed = new HashSet<string>(LearnerHelpers.DeserializeList(learner.CompletedLessonsJson));
            foreach (var s in dto.CompletedLessons ?? new()) if (!string.IsNullOrWhiteSpace(s)) completed.Add(s.Trim());

            // merge days + always count today as active (visiting = learning)
            var days = StreakCalc.UnionDays(
                LearnerHelpers.DeserializeList(learner.LearningDaysJson),
                (dto.Days ?? new()).Append(DateTime.UtcNow.ToString("yyyy-MM-dd")));

            learner.CompletedLessonsJson = System.Text.Json.JsonSerializer.Serialize(completed.ToList());
            learner.LearningDaysJson = System.Text.Json.JsonSerializer.Serialize(days);
            learner.CurrentStreak = StreakCalc.Current(days);
            learner.LongestStreak = Math.Max(learner.LongestStreak, StreakCalc.Longest(days));
            learner.LastActiveDate = DateTime.UtcNow.Date;

            await learners.UpdateAsync(learner);
            return Ok(LearnerHelpers.Profile(learner));
        }
    }
}
