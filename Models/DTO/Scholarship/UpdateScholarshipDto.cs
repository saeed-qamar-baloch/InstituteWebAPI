using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Scholarship
{
    public class UpdateScholarshipDto
    {
        [Required]
        [Range(1, 100, ErrorMessage = "DiscountPercent must be between 1 and 100.")]
        public int DiscountPercent { get; set; }

        [Required]
        public DateTime FromMonth { get; set; }

        [Required]
        public DateTime ToMonth { get; set; }

        public string? Reason { get; set; }

        /// <summary>1 = Active, 2 = Inactive</summary>
        [Required]
        public int Status { get; set; }
    }
}
