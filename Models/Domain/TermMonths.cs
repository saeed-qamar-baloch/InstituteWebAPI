using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    public class TermMonths
    {
        [Key]
        public Guid TermMonthID { get; set; }
        public int TermMonth { get; set;}
        public List<Tests> Tests { get; set; }

    }
}
