using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// A self-service "Learn English" learner account. COMPLETELY SEPARATE from the
    /// portal's ASP.NET Identity users (students/teachers/admins). Lives in the
    /// "web" schema. Stores lesson progress and a learning-day streak.
    /// </summary>
    [Table("Learners", Schema = "web")]
    public class Learner
    {
        [Key]
        public Guid LearnerID { get; set; }

        [Required, MaxLength(120)]
        public string DisplayName { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        /// <summary>Null for Google-only accounts.</summary>
        public string? PasswordHash { get; set; }

        /// <summary>Google account subject id, for "Sign in with Google".</summary>
        [MaxLength(120)]
        public string? GoogleSubject { get; set; }

        /// <summary>JSON array of completed lesson slugs.</summary>
        public string CompletedLessonsJson { get; set; } = "[]";

        /// <summary>JSON array of learning days (YYYY-MM-DD) used to compute the streak.</summary>
        public string LearningDaysJson { get; set; } = "[]";

        public int CurrentStreak { get; set; } = 0;
        public int LongestStreak { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastActiveDate { get; set; }
    }
}
