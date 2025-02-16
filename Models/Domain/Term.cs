using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    public class Term
    {
        [Key]
        public Guid TermID { get; set; }
        public string TermName { get; set; }
        public DateTime TermStart { get; set; } 
        public DateTime TermEnd { get; set; }
        public string TermDuration { get; set; }
        public bool IsActive { get; set; }
        public List<CurrentClass> CurrentClasses { get; set; }
        public List<Tests> Tests { get; set; }
        public List<StudentMarks> StudentMarks { get; set; }

    }
}
