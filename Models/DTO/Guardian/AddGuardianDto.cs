using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Guardian
{
    public class AddGuardianDto
    {
        [Required]
        public Guid StudentID { get; set; }

        [Required]
        public string GuardianName { get; set; }

        /// <summary>Father, Mother, Uncle, Brother, etc.</summary>
        [Required]
        public string Relation { get; set; }

        [Required]
        public string Contact { get; set; }

        public string? Cnic { get; set; }
        public string? Address { get; set; }
        public string? Occupation { get; set; }
        public string? Remarks { get; set; }
    }
}
