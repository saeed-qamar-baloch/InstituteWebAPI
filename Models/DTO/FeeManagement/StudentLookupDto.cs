namespace InstituteWebAPI.Models.DTO.FeeManagement
{
    public class StudentLookupDto
    {
        public Guid StudentId { get; set; }
        public string RegistrationNo { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
    }
}
