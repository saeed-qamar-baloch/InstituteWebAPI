using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.TermMonths
{
    public class TermMonthsUpdateRequestDto
    {
        [Required]
        public int TermMonth { get; set; }

    }
}
