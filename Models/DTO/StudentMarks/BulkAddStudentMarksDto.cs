using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.StudentMarks
{
    public class BulkAddStudentMarksDto
    {
        [Required]
        public Guid CurrentClassID { get; set; }

        [Required]
        public Guid TermMonthID { get; set; }

        [Required]
        public Guid TestID { get; set; }

        [Required]
        public Guid TermID { get; set; }

        [Required]
        public List<BulkMarkItemDto> Items { get; set; } = new();
    }

    public class BulkMarkItemDto
    {
        [Required]
        public Guid StudentID { get; set; }

        [Required]
        public float ObtainedMarks { get; set; }
    }
}
