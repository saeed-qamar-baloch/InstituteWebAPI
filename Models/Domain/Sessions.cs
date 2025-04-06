using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    public class Sessions
    {
        [Key]
        public Guid SessionID { get; set; }
        public string SessionName { get; set; }
        public DateTime SessionStartDate { get; set; }
        public DateTime SessionEndDate { get; set; }
        public bool IsActive { get; set; }
        public List<CurrentClass> CurrentClasses { get; set; }
        public List<Sections> Sections { get; set; }

    }
}
