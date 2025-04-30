using InstituteWebAPI.Models.DTO.CurrentClasses;
using InstituteWebAPI.Models.DTO.Students;

namespace InstituteWebAPI.Models.DTO.ClassStudents
{
    public class ClassStudentDto
    {
        public Guid ClassStudentID { get; set; }
        public string Status { get; set; }

        public StudentDto Student { get; set; }
        public CurrentClassDto CurrentClass { get; set; }
    }
}
