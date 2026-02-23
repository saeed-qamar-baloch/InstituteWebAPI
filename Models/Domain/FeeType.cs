using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    public class FeeType
    {
        [Key]
        public Guid FeeTypeID { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ModifiedAt { get; set; }
    }
}
