using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.StudentMarks
{
    public class AddStudentMarksDto
    {
        [Required]
        public float ObtainedMarks { get; set; }
        [Required]
        public Guid TestID { get; set; }
        [Required]
        public Guid StudentID { get; set; }

        public Guid TermID { get; set; }
    }
}
