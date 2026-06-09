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

        /// <summary>The class the student is placed into at admission time.</summary>
        public Guid? AdmittedClassID { get; set; }

        public DateTime? LeavingDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MonthlyFee { get; set; }

        [Range(0, double.MaxValue)]
        public decimal AdmissionFee { get; set; }

        /// <summary>Day of month (1-31) when fee is due each month.</summary>
        [Range(1, 31)]
        public int? DueDate { get; set; }

        public string Status { get; set; } = "Active";

        public bool IsActive { get; set; } = true;

        /// <summary>When true, no fee is charged (no dues generated).</summary>
        public bool IsFree { get; set; } = false;
    }
}
