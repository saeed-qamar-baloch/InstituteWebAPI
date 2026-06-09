using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Data
{
    public class RozhnInstituteDbContext : DbContext
    {
        public RozhnInstituteDbContext(DbContextOptions<RozhnInstituteDbContext> dbContextOptions) : base(dbContextOptions)
        {
        }

        // ── Existing tables ──────────────────────────────────────────────────
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
        public DbSet<FeeType> FeeTypes { get; set; }
        public DbSet<StudentAttendance> StudentAttendances { get; set; }
        public DbSet<FeeDue> FeeDues { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentDetail> PaymentDetails { get; set; }
        public DbSet<FeeSettings> FeeSettings { get; set; }

        // ── New tables ───────────────────────────────────────────────────────
        public DbSet<Guardian> Guardians { get; set; }
        public DbSet<StudentFeeHistory> StudentFeeHistories { get; set; }
        public DbSet<Scholarship> Scholarships { get; set; }
        public DbSet<ResultApproval> ResultApprovals { get; set; }
        public DbSet<MarkEditRequest> MarkEditRequests { get; set; }
        public DbSet<TeacherDailyAttendance> TeacherDailyAttendances { get; set; }
        public DbSet<TerminalPassingMark> TerminalPassingMarks { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AppNotification> AppNotifications { get; set; }
        public DbSet<TestType> TestTypes { get; set; }
        public DbSet<StudentLeaveRequest> StudentLeaveRequests { get; set; }
        public DbSet<GradeCriteria> GradeCriterias { get; set; }
        public DbSet<TeacherSalary> TeacherSalaries { get; set; }
        public DbSet<TestSchedule> TestSchedules { get; set; }
        public DbSet<AdmitCard> AdmitCards { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<CardRequest> CardRequests { get; set; }
        public DbSet<TimetableEntry> TimetableEntries { get; set; }
        public DbSet<InstituteSetting> InstituteSettings { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One salary record per teacher per month/year.
            modelBuilder.Entity<TeacherSalary>()
                .HasIndex(x => new { x.TeacherID, x.SalaryYear, x.SalaryMonth })
                .IsUnique();

            // ── Seed default grade criteria ───────────────────────────────────
            modelBuilder.Entity<GradeCriteria>().HasData(
                new GradeCriteria { GradeCriteriaID = Guid.Parse("a1000000-0000-0000-0000-000000000001"), GradeLabel = "A", MinPercentage = 80, Description = "Excellent",      DisplayOrder = 1 },
                new GradeCriteria { GradeCriteriaID = Guid.Parse("a1000000-0000-0000-0000-000000000002"), GradeLabel = "B", MinPercentage = 70, Description = "Very Good",       DisplayOrder = 2 },
                new GradeCriteria { GradeCriteriaID = Guid.Parse("a1000000-0000-0000-0000-000000000003"), GradeLabel = "C", MinPercentage = 60, Description = "Good",            DisplayOrder = 3 },
                new GradeCriteria { GradeCriteriaID = Guid.Parse("a1000000-0000-0000-0000-000000000004"), GradeLabel = "D", MinPercentage = 50, Description = "Pass",            DisplayOrder = 4 },
                new GradeCriteria { GradeCriteriaID = Guid.Parse("a1000000-0000-0000-0000-000000000005"), GradeLabel = "E", MinPercentage = 45, Description = "Marginal Pass",   DisplayOrder = 5 },
                new GradeCriteria { GradeCriteriaID = Guid.Parse("a1000000-0000-0000-0000-000000000006"), GradeLabel = "F", MinPercentage = 0,  Description = "Fail",            DisplayOrder = 6 }
            );

            // ── Existing indexes ─────────────────────────────────────────────

            modelBuilder.Entity<StudentMonthlyResult>()
                .HasIndex(x => new { x.TermID, x.CurrentClassID, x.TermMonthID, x.StudentID })
                .IsUnique();

            modelBuilder.Entity<TerminalResult>()
                .HasIndex(x => new { x.TermID, x.CurrentClassID, x.StudentID })
                .IsUnique();

            modelBuilder.Entity<StudentAttendance>()
                .HasIndex(x => new { x.AttendanceDate, x.CurrentClassID, x.StudentID })
                .IsUnique();

            modelBuilder.Entity<FeeDue>()
                .HasIndex(x => new { x.AdmissionId, x.FeeType, x.FeeMonth })
                .IsUnique()
                .HasFilter("[FeeMonth] IS NOT NULL");

            modelBuilder.Entity<PaymentDetail>()
                .HasIndex(x => x.FeeDueId);

            modelBuilder.Entity<PaymentDetail>()
                .HasOne(pd => pd.Payment)
                .WithMany(p => p.PaymentDetails)
                .HasForeignKey(pd => pd.PaymentId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── New indexes ──────────────────────────────────────────────────

            // One guardian record per student (can be extended to multi-guardian later)
            modelBuilder.Entity<Guardian>()
                .HasIndex(x => x.StudentID);

            // One active fee record per admission at a time
            modelBuilder.Entity<StudentFeeHistory>()
                .HasIndex(x => new { x.AdmissionID, x.EffectiveFrom });

            // One terminal passing mark per (TermID, CurrentClassID)
            modelBuilder.Entity<TerminalPassingMark>()
                .HasIndex(x => new { x.TermID, x.CurrentClassID })
                .IsUnique();

            // One result approval record per (TermID, CurrentClassID)
            modelBuilder.Entity<ResultApproval>()
                .HasIndex(x => new { x.TermID, x.CurrentClassID })
                .IsUnique();

            // One attendance record per teacher per day
            modelBuilder.Entity<TeacherDailyAttendance>()
                .HasIndex(x => new { x.TeacherID, x.AttendanceDate })
                .IsUnique();

            // Admissions: FK to AdmittedClass — no cascade delete (class shouldn't be deletable while referenced)
            modelBuilder.Entity<Admissions>()
                .HasOne(a => a.AdmittedClass)
                .WithMany()
                .HasForeignKey(a => a.AdmittedClassID)
                .OnDelete(DeleteBehavior.NoAction);

            // CardRequest: optional FK to the requesting teacher — no cascade delete
            modelBuilder.Entity<CardRequest>()
                .HasOne(c => c.RequestedByTeacher)
                .WithMany()
                .HasForeignKey(c => c.RequestedByTeacherID)
                .OnDelete(DeleteBehavior.NoAction);

            // MarkEditRequest: no cascade on StudentMark delete
            modelBuilder.Entity<MarkEditRequest>()
                .HasOne(m => m.StudentMark)
                .WithMany()
                .HasForeignKey(m => m.StudentMarkID)
                .OnDelete(DeleteBehavior.NoAction);

            // Scholarship: no cascade on Admission delete
            modelBuilder.Entity<Scholarship>()
                .HasOne(s => s.Admission)
                .WithMany(a => a.Scholarships)
                .HasForeignKey(s => s.AdmissionID)
                .OnDelete(DeleteBehavior.NoAction);

            // StudentFeeHistory: no cascade on Admission delete
            modelBuilder.Entity<StudentFeeHistory>()
                .HasOne(f => f.Admission)
                .WithMany(a => a.FeeHistories)
                .HasForeignKey(f => f.AdmissionID)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
