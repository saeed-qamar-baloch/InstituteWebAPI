namespace InstituteWebAPI.Models.DTO.FeeManagement
{
    public class FeeSettingsDto
    {
        public decimal LateFeeAmount { get; set; }
        public decimal AdmissionFeeAmount { get; set; }
        public decimal CardFeeAmount { get; set; }
    }
}
