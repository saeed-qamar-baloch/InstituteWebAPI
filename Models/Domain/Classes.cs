using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstituteWebApp.Models.Domain
{
    public class Classes
    {
        [Key]
        public Guid ClassID { get; set; }

        public string ClassName { get; set; }

        /// <summary>
        /// Progression order within a course (1 = lowest level).
        /// Used by the promotion engine to find the "next" class a passing
        /// student moves into (e.g. Basic A = 1, Basic B = 2, Foundation = 3 …).
        /// 0 means "unranked".
        /// </summary>
        public int Rank { get; set; }

        public Guid CourseID { get; set; }
        [ForeignKey("CourseID")]
        public Courses Course { get; set; }
        public List<CurrentClass> CurrentClasses { get; set; }
    }
}
