using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class FeeSettings
    {
        [Key]
        public Guid FeeSettingsId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LateFeeAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdmissionFeeAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CardFeeAmount { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
