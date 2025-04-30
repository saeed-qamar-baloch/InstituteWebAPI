using System.ComponentModel.DataAnnotations;

public class AddTeacherDto
{

    public string RegistrationNo { get; set; }
    [Required]
    public string TeacherName { get; set; }
    [Required]
    public string FatherName { get; set; }
    [Required]
    public string Gender { get; set; }
    [Required]
    public DateTime DateOfBirth { get; set; }
    [Required]
    public string Address { get; set; }
    [Required]
    public string City { get; set; }
    [Required]
    public string Region { get; set; }
    [Required]
    public string? EmergencyContact { get; set; }
    [Required]
    public string Contact { get; set; }
    [Required]
    public string FatherOccupation { get; set; }
    [Required]
    public string Qualification { get; set; }
    [Required]
    public string Institute { get; set; }

    public string? Cnic { get; set; }
    public string? Picture { get; set; }
    public string? Experience { get; set; }
    public DateTime RegistrationDate { get; set; }
    public bool IsTeaching { get; set; }
}
