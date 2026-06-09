using InstituteWebAPI.Models.DTO.Courses;
using InstituteWebApp.Models.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebAPI.Models.DTO.Classes
{
    public class ClassesDto
    {
        public Guid ClassID { get; set; }

        public string ClassName { get; set; }
        public int Rank { get; set; }
        public CourseDto Course { get; set; }
    }
}
