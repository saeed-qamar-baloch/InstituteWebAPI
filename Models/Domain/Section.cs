using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    public class Section
    {
        [Key]
        public Guid SectionID { get; set; }

        public string Name { get; set; }

        public Guid? TermID { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey("TermID")]
        public Term? Term { get; set; }
    }
}
