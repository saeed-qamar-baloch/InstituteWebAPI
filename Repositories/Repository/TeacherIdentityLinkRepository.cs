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
            // The login account link is stored in IdentityUserId so that
            // RegistrationNo stays a human-readable code (e.g. LT-Feb26-001).
            return await dbContext.Teachers
                .AsNoTracking()
                .Where(t => t.IdentityUserId == userId)
                .Select(t => (Guid?)t.TeacherID)
                .FirstOrDefaultAsync();
        }

        public async Task LinkTeacherToUserIdAsync(Guid teacherId, string userId)
        {
            var teacher = await dbContext.Teachers.FirstOrDefaultAsync(t => t.TeacherID == teacherId);
            if (teacher == null) throw new InvalidOperationException("Teacher not found");

            teacher.IdentityUserId = userId;
            await dbContext.SaveChangesAsync();
        }
    }
}
