using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Classes
{
    public class AddClassesDto
    {
        //public Guid ClassID { get; set; }
        [Required]
        public string ClassName { get; set; }
        [Required]
        public Guid CourseID { get; set; }

        /// <summary>Progression order within the course (1 = lowest). 0 = unranked.</summary>
        public int Rank { get; set; }
    }
}
