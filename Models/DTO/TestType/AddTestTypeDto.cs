using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.TestType
{
    public class AddTestTypeDto
    {
        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        /// <summary>True = restrict to the active term; false = visible in every term.</summary>
        public bool CurrentTermOnly { get; set; }
    }
}
