namespace InstituteWebAPI.Models.DTO
{
    public class AddSessionsDto
    {
        public string SessionName { get; set; }
        public DateTime SessionStartDate { get; set; }
        public DateTime SessionEndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
