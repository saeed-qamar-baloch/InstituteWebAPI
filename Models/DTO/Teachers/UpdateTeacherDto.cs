using System.ComponentModel.DataAnnotations;

public class UpdateTeacherDto
{
    public Guid TeacherID { get; set; }
    public string? RegistrationNo { get; set; }
    public string TeacherName { get; set; }
    public string FatherName { get; set; }
    public string Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Region { get; set; }
    public string EmergencyContact { get; set; }
    public string Contact { get; set; }
    public string? FatherOccupation { get; set; }
    public string Qualification { get; set; }
    public string Institute { get; set; }
    public string? Cnic { get; set; }
    public string? Picture { get; set; }
    public string? Experience { get; set; }
    public DateTime RegistrationDate { get; set; }
    public bool IsTeaching { get; set; }
    public IFormFile? file { get; set; }
}
