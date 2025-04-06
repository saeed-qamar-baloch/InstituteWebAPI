using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class Tests
    {
        [Key]
        public Guid TestID { get; set; }
        public Guid TermMonthID { get; set; }
        [ForeignKey("TermMonthID")]
        public TermMonths TermMonth { get; set; }
        public string TestType { get; set; }
        public float TotalMarks { get; set; }
        public Guid CurrentClassID { get; set; }
        [ForeignKey("CurrentClassID")]
        public CurrentClass CurrentClass { get; set; }
        public List<StudentMarks> StudentMarks { get; set; }

    }
}
