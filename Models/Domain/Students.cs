using InstituteWebApp.Models.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Reflection.Metadata;
namespace InstituteWebApp.Models.Domain
{
    public class Students
    {
        [Key]
        public Guid StudentID { get; set; }
        public int Serial { get; set; }  // New column for serial number
        public DateTime RegDate { get; set; }  // New column for registration date

        public string RegistrationNo { get; set; }

        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Guid VillageID { get; set; }
        [ForeignKey("VillageID")]
        public Village Village { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string FatherContact { get; set; }
        public string? StudentContact { get; set; }
        public string FatherOccupation { get; set; }
        public string Qualification { get; set; }
        public string Institute { get; set; }
        public string? FatherCnic { get; set; }
        public string? Picture { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool IsEnrolled { get; set; }
        public string Remarks { get; set; }
        [NotMapped]
        public IFormFile file { get; set; }



        public List<Admissions> Admissions { get; set; }
        public List<ClassStudents> ClassStudents { get; set; }
        public List<StudentMarks> StudentMarks { get; set; }
    }
}