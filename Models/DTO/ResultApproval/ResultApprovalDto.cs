namespace InstituteWebAPI.Models.DTO.ResultApproval
{
    public class ResultApprovalDto
    {
        public Guid ApprovalID { get; set; }
        public Guid TermID { get; set; }
        public string? TermName { get; set; }
        public Guid CurrentClassID { get; set; }
        public string? ClassName { get; set; }
        public bool IsApproved { get; set; }
        public string? ApprovedByUserID { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
