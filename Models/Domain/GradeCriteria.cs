using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// Configurable grade boundaries used system-wide for all terminal results.
    /// Grade is assigned to a student whose percentage >= MinPercentage.
    /// Sort by MinPercentage DESC to find the first matching grade.
    /// </summary>
    public class GradeCriteria
    {
        [Key]
        public Guid GradeCriteriaID { get; set; }

        /// <summary>Grade label shown on result card (e.g. "A", "B", "C", "D", "E", "F")</summary>
        [Required, MaxLength(10)]
        public string GradeLabel { get; set; } = "";

        /// <summary>Minimum percentage (inclusive) to earn this grade</summary>
        public float MinPercentage { get; set; }

        /// <summary>Optional description shown in settings (e.g. "Excellent", "Pass")</summary>
        [MaxLength(50)]
        public string? Description { get; set; }

        /// <summary>UI sort order — lower numbers appear first (A=1, B=2 …)</summary>
        public int DisplayOrder { get; set; }
    }
}
