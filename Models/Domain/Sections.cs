using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class Sections
    {
        [Key]
        public Guid SectionID { get; set; }
        public string SectionName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set;}
        public Guid? TermID { get; set; }
        [ForeignKey("TermID")]
        public Term? term { get; set; }
        public Guid CourseID { get; set; }
        [ForeignKey("CourseID")]
        public Courses Course { get; set; }

        public Guid? SessionID { get; set;}
        [ForeignKey("SessionID")]
        public Sessions? Sessions { get; set; }

        public List<CurrentClass> CurrentClasses { get; set; }

    }
}
