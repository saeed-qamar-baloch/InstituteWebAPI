using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Learners
{
    public class RegisterLearnerDto
    {
        [Required, MaxLength(120)]
        public string DisplayName { get; set; }

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }
    }

    public class LoginLearnerDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class GoogleLoginDto
    {
        /// <summary>The Google ID-token credential returned by Google Identity Services.</summary>
        [Required]
        public string Credential { get; set; }
    }

    public class SyncProgressDto
    {
        public List<string> CompletedLessons { get; set; } = new();
        public List<string> Days { get; set; } = new();
    }
}
