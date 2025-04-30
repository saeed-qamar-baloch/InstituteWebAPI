using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.CurrentClasses
{
    public class UpdateCurrentClassDto
    {
        [Required]
        public Guid ClassID { get; set; }
        public Guid? SectionID { get; set; }
        public Guid? TeacherID { get; set; }
        public Guid? SessionID { get; set; }
        public Guid? TermID { get; set; }
        public bool IsActive { get; set; }
    }
}
