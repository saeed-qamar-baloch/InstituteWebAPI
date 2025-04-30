using InstituteWebAPI.Models.DTO.Courses;
using InstituteWebAPI.Models.DTO.Students;

namespace InstituteWebAPI.Models.DTO.Admissions
{
    public class AdmissionDto
    {
        public Guid AdmissionID { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime LeavingDate { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }

        public StudentDto Student { get; set; }
        public CourseDto Course { get; set; }
    }
}
