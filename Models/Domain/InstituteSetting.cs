using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// Single-row institute-wide settings (extensible).
    /// OffDays: comma-separated day numbers that are weekly holidays (1=Mon … 7=Sun).
    /// </summary>
    public class InstituteSetting
    {
        [Key]
        public Guid InstituteSettingID { get; set; }

        public string? OffDays { get; set; }

        public string? InstituteName { get; set; }
        public string? LogoUrl { get; set; }
        public string? Tagline { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? SinceYear { get; set; }
    }
}
