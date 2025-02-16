using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class TimeSlot
    {
        [Key]
        public Guid TimeSlotID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set;}
        public Guid? TermID { get; set; }
        [ForeignKey("TermID")]
        public Term? term { get; set; }
        public List<CurrentClass> CurrentClasses { get; set; }

    }
}
