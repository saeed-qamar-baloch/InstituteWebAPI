// UpdateStudentDto.cs
namespace InstituteWebAPI.Models.DTO
{
    public class UpdateStudentDto
    {
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string FatherContact { get; set; }
        public string? StudentContact { get; set; }
        public string Qualification { get; set; }
        public string Institute { get; set; }
        public string? FatherCnic { get; set; }
        public string? Picture { get; set; }
        public string Remarks { get; set; }
    }
}
