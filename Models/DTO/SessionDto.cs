namespace InstituteWebAPI.Models.DTO
{
    public class SessionDto
    {
        public Guid SessionID { get; set; }
        public string SessionName { get; set; }
        public DateTime SessionStartDate { get; set; }
        public DateTime SessionEndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
