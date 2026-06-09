using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// Records who performed a sensitive action (fees, marks, promotions, expenses…)
    /// for accountability.
    /// </summary>
    public class AuditLog
    {
        [Key]
        public Guid AuditLogID { get; set; }

        public string? UserName { get; set; }
        public string? UserId { get; set; }
        public string? Role { get; set; }

        /// <summary>High-level module: Fees | Marks | Promotion | Expense | Settings …</summary>
        public string Module { get; set; } = "";

        /// <summary>Short action name, e.g. "Fee Collected", "Terminal Result Changed".</summary>
        public string Action { get; set; } = "";

        /// <summary>Optional human-readable details / summary of the change.</summary>
        public string? Details { get; set; }

        public string? EntityType { get; set; }
        public string? EntityId { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
