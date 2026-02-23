using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Models.DTO.FeeManagement
{
    public class PaymentSummaryDto
    {
        public Guid PaymentId { get; set; }
        public Guid StudentId { get; set; }
        public string RegistrationNo { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}
