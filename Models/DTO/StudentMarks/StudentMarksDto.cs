using InstituteWebAPI.Models.DTO.Students;
using InstituteWebAPI.Models.DTO.Terms;
using InstituteWebAPI.Models.DTO.Tests;

namespace InstituteWebAPI.Models.DTO.StudentMarks
{
    public class StudentMarksDto
    {
        public Guid StudentMarkID { get; set; }
        public float ObtainedMarks { get; set; }
        public TestDto Test { get; set; }
        public StudentDto Student { get; set; }
        public TermDto Term { get; set; }
    }
}
