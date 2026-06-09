namespace InstituteWebAPI.Models.DTO.StudentLeaveRequest
{
    public class StudentLeaveRequestDto
    {
        public Guid StudentLeaveRequestID { get; set; }
        public Guid StudentID { get; set; }
        public string StudentName { get; set; }
        public string RegistrationNo { get; set; }
        public Guid CurrentClassID { get; set; }
        public string ClassName { get; set; }
        public string SlotName { get; set; }
        public Guid AdmissionID { get; set; }
        public Guid RequestedByTeacherID { get; set; }
        public string RequestedByTeacherName { get; set; }
        public DateTime LeavingDate { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string? ReviewedByUserID { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewRemarks { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
