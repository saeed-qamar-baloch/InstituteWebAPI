namespace InstituteWebAPI.Models.DTO.MarkEditRequest
{
    public class MarkEditRequestDto
    {
        public Guid RequestID { get; set; }

        public Guid TeacherID { get; set; }
        public string? TeacherName { get; set; }

        public Guid StudentMarkID { get; set; }
        public string? StudentName { get; set; }
        public string? RegistrationNo { get; set; }
        public string? TestName { get; set; }

        public float CurrentMarks { get; set; }
        public float RequestedMarks { get; set; }
        public float TotalMarks { get; set; }       // from StudentMark for context

        public string Reason { get; set; }
        public string Status { get; set; }          // "Pending" | "Approved" | "Rejected"

        public string? ReviewedByUserID { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewRemarks { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
