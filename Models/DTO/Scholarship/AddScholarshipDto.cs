using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Scholarship
{
    public class AddScholarshipDto
    {
        [Required]
        public Guid StudentID { get; set; }

        [Required]
        public Guid AdmissionID { get; set; }

        /// <summary>Must be 25, 50, or 100.</summary>
        [Required]
        [Range(1, 100, ErrorMessage = "DiscountPercent must be between 1 and 100.")]
        public int DiscountPercent { get; set; }

        /// <summary>First day of the first month the scholarship applies.</summary>
        [Required]
        public DateTime FromMonth { get; set; }

        /// <summary>First day of the last month the scholarship applies.</summary>
        [Required]
        public DateTime ToMonth { get; set; }

        public string? Reason { get; set; }

        /// <summary>True for a leave (full waiver) rather than a merit scholarship.</summary>
        public bool IsLeave { get; set; }
    }
}
