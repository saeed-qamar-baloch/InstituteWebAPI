using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Terms
{
    public class TermUpdateRequestDto
    {
        public Guid TermID { get; set; }
        [Required]
        public string TermName { get; set; }
        [Required]
        public DateTime TermStart { get; set; }
        [Required]
        public DateTime TermEnd { get; set; }
        public string TermDuration { get; set; }
        public bool IsActive { get; set; }
    }
}
