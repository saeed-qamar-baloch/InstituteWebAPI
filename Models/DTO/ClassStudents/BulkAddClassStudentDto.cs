using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.ClassStudents
{
    public class BulkAddClassStudentDto
    {
        [Required]
        public Guid CurrentClassID { get; set; }

        [Required, MinLength(1)]
        public List<Guid> StudentIDs { get; set; } = new();

        public string Status { get; set; } = "Active";
    }

    public class BulkAddResultDto
    {
        public int Assigned { get; set; }
        public int Skipped  { get; set; }
        public List<string> SkippedNames { get; set; } = new();
    }
}
