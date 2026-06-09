namespace InstituteWebAPI.Models.DTO.Admissions
{
    public class AdmissionDto
    {
        public Guid AdmissionID { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LeavingDate { get; set; }
        public decimal MonthlyFee { get; set; }
        public decimal AdmissionFee { get; set; }
        public int? DueDate { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsFree { get; set; }

        // Flattened student fields (avoids circular navigation issues)
        public Guid StudentID { get; set; }
        public string StudentName { get; set; }
        public string RegistrationNo { get; set; }
        public string FatherName { get; set; }
        public string? Picture { get; set; }
        public string? FatherContact { get; set; }

        // Flattened course + class fields
        public Guid CourseID { get; set; }
        public string CourseName { get; set; }
        public Guid? AdmittedClassID { get; set; }
        public string? ClassName { get; set; }
    }
}
