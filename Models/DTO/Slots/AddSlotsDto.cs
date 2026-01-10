using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Slots
{
    public class AddSlotsDto
    {
        [Required]
        public string SlotName { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [Required]
        public Guid CourseID { get; set; }

        public Guid? TermID { get; set; }
        public Guid? SessionID { get; set; }
    }
}
