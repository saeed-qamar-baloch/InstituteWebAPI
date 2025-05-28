using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Villages
{
    public class VillageUpdateRequestDto
    {
        public Guid VillageID { get; set; }
        [Required]
        public string VillageName { get; set; }

    }
}
