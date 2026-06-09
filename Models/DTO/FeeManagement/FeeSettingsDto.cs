namespace InstituteWebAPI.Models.DTO.FeeManagement
{
    public class FeeSettingsDto
    {
        public decimal LateFeeAmount { get; set; }
        public decimal AdmissionFeeAmount { get; set; }
        public decimal CardFeeAmount { get; set; }

        // Earliest month for which monthly dues are generated. Null = no floor.
        public int? FeeStartYear { get; set; }
        public int? FeeStartMonth { get; set; }   // 1-12
    }
}
