using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.StudentLeaveRequest
{
    public class CreateLeaveRequestDto
    {
        [Required]
        public Guid StudentID { get; set; }

        [Required]
        public Guid CurrentClassID { get; set; }

        // Optional — if omitted the controller resolves the student's active admission automatically
        public Guid? AdmissionID { get; set; }

        [Required]
        public DateTime LeavingDate { get; set; }

        [Required]
        public string Reason { get; set; }
    }
}
