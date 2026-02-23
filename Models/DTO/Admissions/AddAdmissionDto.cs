using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.Admissions
{
    public class AddAdmissionDto
    {
        [Required]
        public Guid StudentID { get; set; }
        [Required]
        public DateTime RegistrationDate { get; set; }
        [Required]
        public Guid CourseID { get; set; }
        public DateTime LeavingDate { get; set; }
        // Monthly fee (decimal) and due date as day-of-month (1-31)
        public decimal MonthlyFee { get; set; }
        public int? DueDate { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }
}
