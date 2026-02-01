using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Data
{
    public class RozhnInstituteDbContext : DbContext
    {
        public RozhnInstituteDbContext(DbContextOptions<RozhnInstituteDbContext> dbContextOptions) : base(dbContextOptions)
        {
        }

        public DbSet<Classes> Classes { get; set; }
        public DbSet<ClassStudents> ClassStudents { get; set; }
        public DbSet<Courses> Courses { get; set; }
        public DbSet<CurrentClass> CurrentClasses { get; set; }
        public DbSet<Sessions> Sessions { get; set; }
        public DbSet<Admissions> Admissions { get; set; }
        public DbSet<StudentMarks> StudentMarks { get; set; }
        public DbSet<StudentMonthlyResult> StudentMonthlyResults { get; set; }
        public DbSet<Students> Students { get; set; }
        public DbSet<TeacherCourses> TeacherCourses { get; set; }
        public DbSet<Teachers> Teachers { get; set; }
        public DbSet<Term> Term { get; set; }
        public DbSet<TermMonths> TermMonths { get; set; }
        public DbSet<Tests> Tests { get; set; }
        public DbSet<Slots> Slots { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Village> Village { get; set; }
        public DbSet<TermMonthPassingMark> TermMonthPassingMarks { get; set; }

        public DbSet<TerminalResult> TerminalResults { get; set; }

        public DbSet<StudentAttendance> StudentAttendances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StudentMonthlyResult>()
                .HasIndex(x => new { x.TermID, x.CurrentClassID, x.TermMonthID, x.StudentID })
                .IsUnique();

            modelBuilder.Entity<TerminalResult>()
                .HasIndex(x => new { x.TermID, x.CurrentClassID, x.StudentID })
                .IsUnique();

            modelBuilder.Entity<StudentAttendance>()
                .HasIndex(x => new { x.AttendanceDate, x.CurrentClassID, x.StudentID })
                .IsUnique();
        }
    }
}
