namespace InstituteWebAPI.Models.Configuration
{
    public class FeeManagementOptions
    {
        public decimal LateFeeAmount { get; set; }
        public decimal AdmissionFeeAmount { get; set; }
        public decimal CardFeeAmount { get; set; }
    }
}
