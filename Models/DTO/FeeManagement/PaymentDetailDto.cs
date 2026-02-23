namespace InstituteWebAPI.Models.DTO.FeeManagement
{
    public class PaymentDetailDto
    {
        public Guid PaymentDetailId { get; set; }
        public Guid FeeDueId { get; set; }
        public decimal PaidAmount { get; set; }
    }
}
