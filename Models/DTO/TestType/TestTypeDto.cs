namespace InstituteWebAPI.Models.DTO.TestType
{
    public class TestTypeDto
    {
        public Guid TestTypeID { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public Guid? TermID { get; set; }
        public bool CurrentTermOnly { get; set; }
    }
}
