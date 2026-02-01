using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class TeacherIdentityLinkRepository : ITeacherIdentityLinkRepository
    {
        private readonly RozhnInstituteDbContext dbContext;

        public TeacherIdentityLinkRepository(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Guid?> GetTeacherIdForUserIdAsync(string userId)
        {
            // The requirement is: teacher RegNo is linked with identity.
            // Here we treat Teachers.RegistrationNo as the IdentityUser.Id (string).
            return await dbContext.Teachers
                .AsNoTracking()
                .Where(t => t.RegistrationNo == userId)
                .Select(t => (Guid?)t.TeacherID)
                .FirstOrDefaultAsync();
        }

        public async Task LinkTeacherToUserIdAsync(Guid teacherId, string userId)
        {
            var teacher = await dbContext.Teachers.FirstOrDefaultAsync(t => t.TeacherID == teacherId);
            if (teacher == null) throw new InvalidOperationException("Teacher not found");

            teacher.RegistrationNo = userId;
            await dbContext.SaveChangesAsync();
        }
    }
}
