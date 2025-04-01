using InstituteWebApp.Models.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebAPI.Models.DTO
{
    public class ClassesDto
    {
        public Guid ClassID { get; set; }

        public string ClassName { get; set; }
        public CourseDto Course { get; set; }
    }
}
