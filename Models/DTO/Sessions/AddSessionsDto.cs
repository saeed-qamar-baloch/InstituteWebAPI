using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Sessions
{
    public class AddSessionsDto
    {
        [Required]
        public string SessionName { get; set; }
        [Required]
        public DateTime SessionStartDate { get; set; }
        [Required]
        public DateTime SessionEndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
