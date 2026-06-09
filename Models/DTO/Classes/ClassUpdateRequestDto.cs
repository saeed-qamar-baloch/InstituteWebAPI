namespace InstituteWebAPI.Models.DTO.Classes
{
    public class ClassUpdateRequestDto
    {
        public Guid ClassID { get; set; }
        public string ClassName { get; set; }
        public Guid CourseID { get; set; }

        /// <summary>Progression order within the course (1 = lowest). 0 = unranked.</summary>
        public int Rank { get; set; }
    }
}
