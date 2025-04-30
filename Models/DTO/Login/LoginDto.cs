using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Login
{
    public class LoginDto
    {
        [DataType(DataType.EmailAddress)]
        [Required]
        public string Username { get; set; }
        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }
    }
}
