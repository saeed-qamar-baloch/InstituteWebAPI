// AddStudentDto.cs
using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Students
{
    public class AddStudentDto
    {
        [Required]
        public DateTime RegDate { get; set; }
        [Required]
        public string StudentName { get; set; }
        [Required]
        public string FatherName { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public Guid VillageID { get; set; }
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
        public string? FatherCnic { get; set; }
       public string? Picture { get; set; }
        public bool IsEnrolled { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
    }
}
