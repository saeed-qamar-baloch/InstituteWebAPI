using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.StudentMarks.Terminal
{
    public class UpsertTerminalResultDto
    {
        [Required]
        public Guid CurrentClassID { get; set; }

        [Required]
        public Guid TermID { get; set; }

        [Required]
        public List<UpsertTerminalStudentSettingDto> Students { get; set; } = new();
    }

    public class UpsertTerminalStudentSettingDto
    {
        [Required]
        public Guid StudentID { get; set; }

        public bool IncludeMonth1 { get; set; }
        public bool IncludeMonth2 { get; set; }
    }
}
