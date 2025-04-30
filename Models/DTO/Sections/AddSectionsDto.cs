using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Sections
{
    public class AddSectionsDto
    {
        [Required]
        public string SectionName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        [Required]
        public Guid CourseID { get; set; }
        public Guid? TermID { get; set; }
        public Guid? SessionID { get; set; }
    }
}
