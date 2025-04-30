using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Admissions
{
    public class AddAdmissionDto
    {
        [Required]
        public Guid StudentID { get; set; }
        [Required]
        public DateTime RegistrationDate { get; set; }
        [Required]
        public Guid CourseID { get; set; }
        public DateTime LeavingDate { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }
}
