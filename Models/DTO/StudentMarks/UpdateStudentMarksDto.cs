using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.StudentMarks
{
    public class UpdateStudentMarksDto
    {
        [Required]
        public float ObtainedMarks { get; set; }
        [Required]
        public Guid TestID { get; set; }
        public Guid StudentID { get; set; }
        public Guid TermID { get; set; }
    }
}
