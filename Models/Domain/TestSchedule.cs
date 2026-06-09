using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// A scheduled test for a class: which class, when (date + time), and its status.
    /// Status values: Coming | Conducted | Cancelled | Postponed.
    /// </summary>
    public class TestSchedule
    {
        [Key]
        public Guid TestScheduleID { get; set; }

        public Guid CurrentClassID { get; set; }
        [ForeignKey(nameof(CurrentClassID))]
        public CurrentClass CurrentClass { get; set; }

        /// <summary>The term month this test belongs to (Month 1 / 2 / 3). Optional.</summary>
        public Guid? TermMonthID { get; set; }
        [ForeignKey(nameof(TermMonthID))]
        public TermMonths? TermMonth { get; set; }

        /// <summary>Optional label, e.g. "Math Monthly Test".</summary>
        public string? Title { get; set; }

        /// <summary>Scheduled date and time of the test.</summary>
        public DateTime ScheduledOn { get; set; }

        /// <summary>Coming | Conducted | Cancelled | Postponed.</summary>
        public string Status { get; set; } = "Coming";

        public string? Notes { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }
    }
}
