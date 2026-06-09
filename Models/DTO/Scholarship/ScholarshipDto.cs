namespace InstituteWebAPI.Models.DTO.Scholarship
{
    public class ScholarshipDto
    {
        public Guid ScholarshipID { get; set; }
        public Guid StudentID { get; set; }
        public string? StudentName { get; set; }
        public string? RegistrationNo { get; set; }
        public Guid AdmissionID { get; set; }
        public int DiscountPercent { get; set; }
        public bool IsLeave { get; set; }
        public DateTime FromMonth { get; set; }
        public DateTime ToMonth { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; }          // "Active" | "Inactive"
        public string? CreatedByUserID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
