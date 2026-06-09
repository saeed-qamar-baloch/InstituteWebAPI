using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    /// <summary>
    /// The passing mark threshold used specifically for the terminal (final) exam
    /// of a class within a term. Kept separate from monthly TermMonthPassingMark
    /// because the teacher can set a different bar for the terminal.
    /// Unique per (TermID, CurrentClassID).
    /// </summary>
    public class TerminalPassingMark
    {
        [Key]
        public Guid TerminalPassingMarkID { get; set; }

        public Guid TermID { get; set; }
        [ForeignKey("TermID")]
        public Term Term { get; set; }

        public Guid CurrentClassID { get; set; }
        [ForeignKey("CurrentClassID")]
        public CurrentClass CurrentClass { get; set; }

        public float PassingMarks { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
