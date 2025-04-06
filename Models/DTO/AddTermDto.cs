namespace InstituteWebAPI.Models.DTO
{
    public class AddTermDto
    {
        //public Guid TermID { get; set; }
        public string TermName { get; set; }
        public DateTime TermStart { get; set; }
        public DateTime TermEnd { get; set; }
        public string TermDuration { get; set; }
        public bool IsActive { get; set; }
    }
}
