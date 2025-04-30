using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Data
{
    public class RozhnInstituteAuthDbContext : IdentityDbContext
    {
        public RozhnInstituteAuthDbContext(DbContextOptions<RozhnInstituteAuthDbContext> options) : base(options)
        {

        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var ParentRoleID = "5d0033a8-20e7-4997-8f3b-5769ec3b040b";
            var AdminRoleID = "f4969356-5492-442e-bbc2-d7128a5206ab";
            var TeacherRoleID = "a551991f-c51c-4149-a7cd-3787b6d727e2";
            var StudentRoleID = "18ff3f63-158f-4a29-9963-e5a3db80cb51";



            var roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = ParentRoleID,
                    ConcurrencyStamp = ParentRoleID,
                    Name = "Parent",
                    NormalizedName = "Parent".ToUpper()
                },

                new IdentityRole
                {
                    Id = AdminRoleID,
                    ConcurrencyStamp = AdminRoleID,
                    Name = "Admin",
                    NormalizedName = "Admin".ToUpper()
                },
                new IdentityRole
                {
                    Id =TeacherRoleID,
                    ConcurrencyStamp = TeacherRoleID,
                    Name = "Teacher",
                    NormalizedName = "Teacher".ToUpper()
                },
                new IdentityRole
                {
                    Id =StudentRoleID,
                    ConcurrencyStamp = StudentRoleID,
                    Name = "Student",
                    NormalizedName = "Student".ToUpper()
                }

            };

            builder.Entity<IdentityRole>().HasData(roles);

        }

    }
}
