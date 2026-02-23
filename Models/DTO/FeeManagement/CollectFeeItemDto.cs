namespace InstituteWebAPI.Models.DTO.FeeManagement
{
    public class CollectFeeItemDto
    {
        public Guid FeeDueId { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal LateFeeAmount { get; set; }
        public bool WaiveLateFee { get; set; }
    }
}
