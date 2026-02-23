using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class Admissions
    {
        [Key]
        public Guid AdmissionID { get; set; }
        public Guid StudentID { get; set;}
        [ForeignKey("StudentID")]
        public Students Student { get; set; }
        public DateTime RegistrationDate { get; set; }
        public Guid CourseID { get; set; }
        [ForeignKey("CourseID")]
        public Courses Course { get; set; }
        public DateTime? LeavingDate { get; set; }
        // Monthly fee for the admission
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyFee { get; set; }

        // Optional due date for the monthly fee as day-of-month (1-31)
        public int? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }
}
