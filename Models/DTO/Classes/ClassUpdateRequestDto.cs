namespace InstituteWebAPI.Models.DTO.Classes
{
    public class ClassUpdateRequestDto
    {
        public Guid ClassID { get; set; }
        public string ClassName { get; set; }
        public Guid CourseID { get; set; }
    }
}
