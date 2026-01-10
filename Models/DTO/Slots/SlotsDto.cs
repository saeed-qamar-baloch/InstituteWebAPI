using InstituteWebAPI.Models.DTO.Courses;
using InstituteWebAPI.Models.DTO.Sessions;
using InstituteWebAPI.Models.DTO.Terms;

namespace InstituteWebAPI.Models.DTO.Slots
{
    public class SlotsDto
    {
        public Guid SlotID { get; set; }
        public string SlotName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public CourseDto Course { get; set; }
        public TermDto? Term { get; set; }
        public SessionsDto? Session { get; set; }
    }
}
