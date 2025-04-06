// AddStudentDto.cs
namespace InstituteWebAPI.Models.DTO
{
    public class AddStudentDto
    {
        public int Serial { get; set; }
        public string RegDate { get; set; }
        public string RegistrationNo { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Guid VillageID { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string FatherContact { get; set; }
        public string? StudentContact { get; set; }
        public string FatherOccupation { get; set; }
        public string Qualification { get; set; }
        public string Institute { get; set; }
        public string? FatherCnic { get; set; }
        public string? Picture { get; set; }
        public DateTime AdmissionDate { get; set; }
        public bool IsEnrolled { get; set; }
        public string Remarks { get; set; }
    }
}
