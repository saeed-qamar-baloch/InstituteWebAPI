using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.MarkEditRequest
{
    /// <summary>Admin uses this to approve or reject a pending MarkEditRequest.</summary>
    public class ReviewMarkEditRequestDto
    {
        /// <summary>2 = Approved, 3 = Rejected</summary>
        [Required]
        [Range(2, 3, ErrorMessage = "Status must be 2 (Approved) or 3 (Rejected).")]
        public int Status { get; set; }

        public string? ReviewRemarks { get; set; }
    }
}
