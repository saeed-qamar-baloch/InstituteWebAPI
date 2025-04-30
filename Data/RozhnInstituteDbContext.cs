using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;

namespace InstituteWebAPI.Data
{
    public class RozhnInstituteDbContext:DbContext
    {
        public RozhnInstituteDbContext(DbContextOptions<RozhnInstituteDbContext> dbContextOptions):base(dbContextOptions)
        {
            
        }

        public DbSet<Classes> Classes { get; set; }
        public DbSet<ClassStudents> ClassStudents { get; set; }
        public DbSet<Courses> Courses { get; set; }
        public DbSet<CurrentClass> CurrentClasses { get; set; }
        public DbSet<Sessions> Sessions { get; set; }
        public DbSet<Admissions> Admissions { get; set; }
        public DbSet<StudentMarks> StudentMarks { get; set; }
        public DbSet<Students> Students { get; set; }
        public DbSet<TeacherCourses> TeacherCourses { get; set; }
        public DbSet<Teachers> Teachers { get; set; }
        public DbSet<Term> Term { get; set; }
        public DbSet<TermMonths> TermMonths { get; set; }
        public DbSet<Tests> Tests { get; set; }
        public DbSet<Sections> Sections { get; set; }
        public DbSet<Village> Village { get; set; }

    }
}
