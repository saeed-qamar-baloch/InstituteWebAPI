using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    public class Section
    {
        [Key]
        public Guid SectionID { get; set; }

        public string Name { get; set; }
    }
}
