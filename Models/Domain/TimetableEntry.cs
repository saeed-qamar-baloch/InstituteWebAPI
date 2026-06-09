using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// One cell of the weekly timetable: a class placed on a day + time slot.
    /// DayOfWeek: 1 = Monday … 7 = Sunday.
    /// </summary>
    public class TimetableEntry
    {
        [Key]
        public Guid TimetableEntryID { get; set; }

        public Guid CurrentClassID { get; set; }
        [ForeignKey(nameof(CurrentClassID))]
        public CurrentClass CurrentClass { get; set; }

        public Guid SlotID { get; set; }
        [ForeignKey(nameof(SlotID))]
        public Slots Slot { get; set; }

        public int DayOfWeek { get; set; }

        public string? Room { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
