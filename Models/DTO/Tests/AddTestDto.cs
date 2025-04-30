using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Tests
{
    public class AddTestDto
    {
        [Required]
        public Guid TermMonthID { get; set; }
        [Required]
        public string TestType { get; set; }
        [Required]
        public float TotalMarks { get; set; }
        [Required]
        public Guid CurrentClassID { get; set; }
    }
}
