using InstituteWebAPI.Models.DTO.Classes;
using InstituteWebAPI.Models.DTO.Section;
using InstituteWebAPI.Models.DTO.Sessions;
using InstituteWebAPI.Models.DTO.Slots;
using InstituteWebAPI.Models.DTO.Terms;

namespace InstituteWebAPI.Models.DTO.CurrentClasses
{
    public class CurrentClassDto
    {
        public Guid CurrentClassID { get; set; }
        public ClassesDto Class { get; set; }
        public SlotsDto? Slot { get; set; }
        public SectionDto? Section { get; set; }
        public TeacherDto? Teacher { get; set; }
        public SessionsDto? Session { get; set; }
        public TermDto Term { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
    }
}
