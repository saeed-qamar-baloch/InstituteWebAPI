using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.ClassStudents
{
    public class AddClassStudentDto
    {
        [Required]
        public Guid CurrentClassID { get; set; }
        [Required]
        public Guid StudentID { get; set; }
        public string Status { get; set; }
    }
}
