using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Villages
{
    public class AddVillageDto
    {
        [Required]
        public string VillageName { get; set; }

    }
}
