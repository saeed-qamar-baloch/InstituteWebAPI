namespace InstituteWebAPI.Models.DTO.Admissions
{
    public class UpdateAdmissionDto
    {
        public Guid StudentID { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LeavingDate { get; set; }
        public Guid CourseID { get; set; }
        public Guid? AdmittedClassID { get; set; }
        public decimal MonthlyFee { get; set; }
        public decimal AdmissionFee { get; set; }
        public int? DueDate { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsFree { get; set; }
    }
}
