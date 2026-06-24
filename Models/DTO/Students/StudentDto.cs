// StudentDto.cs
using InstituteWebAPI.Models.DTO.Villages;

namespace InstituteWebAPI.Models.DTO.Students
{
    public class StudentDto
    {
        public Guid StudentID { get; set; }
        public int Serial { get; set; }
        public DateTime RegDate { get; set; }
        public string RegistrationNo { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string FatherContact { get; set; }
        public string? StudentContact { get; set; }
        public string FatherOccupation { get; set; }
        public string Qualification { get; set; }
        public string Institute { get; set; }
        public string? FatherCnic { get; set; }
        public string? Picture { get; set; }
        public bool IsEnrolled { get; set; }
        public string? Status { get; set; }
        public string Remarks { get; set; }

        public VillageDto Village { get; set; }
    }
}
