namespace InstituteWebAPI.Models.DTO.Slots
{
    public class SlotsUpdateDto
    {
        public string SlotName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid CourseID { get; set; }
        public Guid? TermID { get; set; }
        public Guid? SessionID { get; set; }
    }
}
