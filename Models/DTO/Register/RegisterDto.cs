using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Register
{
    public class RegisterDto
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string[] Roles { get; set; }


    }
}
