using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Register
{
    public class ResetTeacherPasswordDto
    {
        [Required]
        public Guid TeacherID { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }
}
