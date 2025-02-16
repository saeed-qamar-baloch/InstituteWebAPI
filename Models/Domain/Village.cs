using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace InstituteWebApp.Models.Domain
{
    public class Village
    {

        [Key]
        public Guid VillageID { get; set; }
        public string VillageName { get; set; }

        public List<Students> Students { get; set; }

    }
}
