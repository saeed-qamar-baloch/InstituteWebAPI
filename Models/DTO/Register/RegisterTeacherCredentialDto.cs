using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Register
{
    public class RegisterTeacherCredentialDto
    {
        [Required]
        public Guid TeacherID { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
