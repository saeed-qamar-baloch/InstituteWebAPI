using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.FeeManagement
{
    public class WaiveMonthsRequestDto
    {
        [Required]
        public Guid StudentId { get; set; }

        /// <summary>First month to waive (any day; normalised to first of month).</summary>
        [Required]
        public DateTime FromMonth { get; set; }

        /// <summary>Last month to waive (any day; normalised to first of month).</summary>
        [Required]
        public DateTime ToMonth { get; set; }

        public string? Reason { get; set; }
    }

    public class WaiveMonthsResultDto
    {
        public int MonthsWaived { get; set; }     // existing dues set to waived
        public int MonthsCreated { get; set; }    // new zero waived dues created
        public int MonthsSkipped { get; set; }    // already paid — left untouched
        public Guid ScholarshipId { get; set; }   // the leave record created
    }

    public class AwardScholarshipRequestDto
    {
        [Required]
        public Guid StudentId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Percent must be between 1 and 100.")]
        public int Percent { get; set; }

        [Required]
        public DateTime FromMonth { get; set; }

        [Required]
        public DateTime ToMonth { get; set; }

        public string? Reason { get; set; }
    }
}
