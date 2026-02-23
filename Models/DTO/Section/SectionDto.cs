namespace InstituteWebAPI.Models.DTO.Section
{
    public class SectionDto
    {
        public Guid SectionID { get; set; }
        public string Name { get; set; }
        public Guid? TermID { get; set; }
    }
}
