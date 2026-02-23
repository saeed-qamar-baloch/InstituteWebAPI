using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Models.DTO.FeeManagement
{
    public class PaymentDto
    {
        public Guid PaymentId { get; set; }
        public Guid StudentId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? Remarks { get; set; }
        public List<PaymentDetailDto> Details { get; set; } = new();
    }
}
