using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    public class TermMonthPassingMark
    {
        [Key]
        public Guid TermMonthPassingMarkID { get; set; }

        public Guid TermID { get; set; }
        public Guid CurrentClassID { get; set; }

        public Guid TermMonthID { get; set; }

        public float PassingMarks { get; set; }

        public Term? Term { get; set; }
        public CurrentClass? CurrentClass { get; set; }
    }
}
