using System.ComponentModel.DataAnnotations;
using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Models.DTO.FeeManagement
{
    public class CollectFeeRequestDto
    {
        [Required]
        public Guid StudentId { get; set; }

        [Required]
        [MinLength(1)]
        public List<CollectFeeItemDto> FeePayments { get; set; } = new();

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        public string? Remarks { get; set; }
    }
}
