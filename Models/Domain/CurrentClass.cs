using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class CurrentClass
    {
        [Key]
        public Guid CurrentClassID { get; set; }
     
        public Guid ClassID { get; set; }
        [ForeignKey("ClassID")]
        public Classes Class { get; set; }

        public Guid? SlotID { get; set; }
        [ForeignKey("SlotID")]
        public Slots? Slot { get; set; }

        public Guid? SectionID { get; set; }
        [ForeignKey("SectionID")]
        public Section? Section { get; set; }

        public Guid? TeacherID { get; set; }
        [ForeignKey("TeacherID")]
        public Teachers? Teacher { get; set; }
        public Guid? SessionID { get; set; }
        [ForeignKey("SessionID")]
        public Sessions Session { get; set; }
        public Guid? TermID { get; set; }
        [ForeignKey("TermID")]
        public Term Term { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
        public string? Room { get; set; }

        public List<Tests> Tests { get; set; }
        public List<ClassStudents> ClassStudents { get; set; }

    }
}
