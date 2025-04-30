using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.TermMonths
{
    public class AddTermMonthsDto
    {
        //  public Guid TermMonthID { get; set; }
        [Required]
        public int TermMonth { get; set; }

    }
}
