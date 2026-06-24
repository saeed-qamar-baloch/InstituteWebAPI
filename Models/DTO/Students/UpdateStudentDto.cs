// UpdateStudentDto.cs
using InstituteWebAPI.Models.DTO.Villages;
using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Students
{
    public class UpdateStudentDto
    {
        public Guid? StudentID { get; set; }
        public string? RegistrationNo { get; set; }
        [Required]
        public string StudentName { get; set; }
        [Required]
        public string FatherName { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string FatherContact { get; set; }
        public string? StudentContact { get; set; }
        [Required]
        public string FatherOccupation { get; set; }
        [Required]
        public string Qualification { get; set; }
        [Required]
        public string Institute { get; set; }
        [Required]
        public string? FatherCnic { get; set; }
        public string? Picture { get; set; }

        public string? Remarks { get; set; }

        public bool IsEnrolled { get; set; }
        public string? Status { get; set; }
       public Guid VillageID { get; set; }
        public IFormFile? file { get; set; }
    }
}
