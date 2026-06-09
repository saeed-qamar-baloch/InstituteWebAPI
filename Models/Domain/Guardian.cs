using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class Guardian
    {
        [Key]
        public Guid GuardianID { get; set; }

        public Guid StudentID { get; set; }
        [ForeignKey("StudentID")]
        public Students Student { get; set; }

        [Required]
        public string GuardianName { get; set; }

        /// <summary>Father, Mother, Uncle, Brother, etc.</summary>
        [Required]
        public string Relation { get; set; }

        [Required]
        public string Contact { get; set; }

        public string? Cnic { get; set; }
        public string? Address { get; set; }
        public string? Occupation { get; set; }
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
