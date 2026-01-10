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
            // NOTE:
            // The domain model `Teachers` currently has no Email/UserId column.
            // This method therefore provides a best-effort lookup.
            // If you add a dedicated column later (e.g. Teachers.IdentityUserId), replace this.

            // Fallback: treat RegistrationNo as identity user id (only if your data was stored that way).
            return await dbContext.Teachers
                .AsNoTracking()
                .Where(t => t.RegistrationNo == userId)
                .Select(t => (Guid?)t.TeacherID)
                .FirstOrDefaultAsync();
        }
    }
}
