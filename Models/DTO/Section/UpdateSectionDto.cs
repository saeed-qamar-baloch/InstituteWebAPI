using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Section
{
    public class UpdateSectionDto
    {
        [Required]
        public string Name { get; set; }

        public Guid? TermID { get; set; }
    }
}
