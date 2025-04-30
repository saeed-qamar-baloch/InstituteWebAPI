using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Villages
{
    public class VillageUpdateRequestDto
    {
        [Required]
        public string VillageName { get; set; }

    }
}
