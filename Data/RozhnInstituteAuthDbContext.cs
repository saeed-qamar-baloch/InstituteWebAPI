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


            var AdminRoleID = "f4969356-5492-442e-bbc2-d7128a5206ab";
            var TeacherRoleID = "a551991f-c51c-4149-a7cd-3787b6d727e2";

            var roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = AdminRoleID,
                    ConcurrencyStamp = AdminRoleID,
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = TeacherRoleID,
                    ConcurrencyStamp = TeacherRoleID,
                    Name = "Teacher",
                    NormalizedName = "TEACHER"
                }
            };

            builder.Entity<IdentityRole>().HasData(roles);
        }

    }
}
