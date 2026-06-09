using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    public class TestType
    {
        [Key]
        public Guid TestTypeID { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        /// <summary>
        /// When set, this test type belongs only to that term ("Current term only").
        /// When null, it is global and visible in every term.
        /// </summary>
        public Guid? TermID { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ModifiedAt { get; set; }
    }
}
