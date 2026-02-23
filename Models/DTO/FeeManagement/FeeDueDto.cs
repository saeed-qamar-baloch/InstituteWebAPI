namespace InstituteWebAPI.Models.DTO.FeeManagement
{
    public class FeeDueDto
    {
        public Guid FeeDueId { get; set; }
        public Guid AdmissionId { get; set; }
        public string FeeType { get; set; }
        public DateTime? FeeMonth { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal LateFeeAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsLateFeeWaived { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
