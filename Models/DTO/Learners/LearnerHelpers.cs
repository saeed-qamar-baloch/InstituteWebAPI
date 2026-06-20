using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InstituteWebApp.Models.Domain;
using Microsoft.IdentityModel.Tokens;

namespace InstituteWebAPI.Models.DTO.Learners
{
    public static class LearnerHelpers
    {
        public static List<string> DeserializeList(string json)
        {
            try { return System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new(); }
            catch { return new(); }
        }

        /// <summary>Public profile shape returned to the Learn site.</summary>
        public static object Profile(Learner l) => new
        {
            id = l.LearnerID,
            displayName = l.DisplayName,
            email = l.Email,
            completedLessons = DeserializeList(l.CompletedLessonsJson),
            days = DeserializeList(l.LearningDaysJson),
            currentStreak = l.CurrentStreak,
            longestStreak = l.LongestStreak,
            lastActiveDate = l.LastActiveDate?.ToString("yyyy-MM-dd"),
        };

        /// <summary>JWT for a learner — same signing as the portal but role "Learner"
        /// only, so it can never reach Admin/Teacher endpoints.</summary>
        public static string CreateToken(Learner l, IConfiguration cfg)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, l.LearnerID.ToString()),
                new Claim(ClaimTypes.Email, l.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, l.DisplayName ?? string.Empty),
                new Claim(ClaimTypes.Role, "Learner"),
                new Claim("account_type", "learner"),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                cfg["Jwt:Issuer"], cfg["Jwt:Audience"], claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
