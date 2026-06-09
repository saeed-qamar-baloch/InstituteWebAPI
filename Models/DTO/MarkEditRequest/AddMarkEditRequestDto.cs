using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.MarkEditRequest
{
    public class AddMarkEditRequestDto
    {
        [Required]
        public Guid StudentMarkID { get; set; }

        [Required]
        [Range(0, float.MaxValue, ErrorMessage = "RequestedMarks must be 0 or greater.")]
        public float RequestedMarks { get; set; }

        [Required]
        public string Reason { get; set; }
    }
}
