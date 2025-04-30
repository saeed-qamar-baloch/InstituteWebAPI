using System.ComponentModel.DataAnnotations;

public class UpdateClassStudentDto
{
    [Required]
    public Guid StudentID { get; set; }
    public Guid CurrentClassID { get; set; }
    public string Status { get; set; }
}
