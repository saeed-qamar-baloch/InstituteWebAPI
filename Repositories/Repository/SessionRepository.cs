using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class SessionRepository : ISessionRepository
    {
        private readonly RozhnInstituteDbContext dbContext;

        public SessionRepository(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Sessions> AddAsync(Sessions session)
        {
            await dbContext.Sessions.AddAsync(session);
            await dbContext.SaveChangesAsync();
            return session;
        }

        public async Task<Sessions?> DeleteAsync(Guid id)
        {
            var existing = await dbContext.Sessions.FirstOrDefaultAsync(s => s.SessionID == id);
            if (existing == null) return null;

            dbContext.Sessions.Remove(existing);
            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<Sessions>> GetAllAsync()
        {
            return await dbContext.Sessions.ToListAsync();
        }

        public async Task<Sessions?> GetAsync(Guid id)
        {
            return await dbContext.Sessions.FirstOrDefaultAsync(s => s.SessionID == id);
        }

        public async Task<Sessions?> GetByNameAsync(string name)
        {
            return await dbContext.Sessions.FirstOrDefaultAsync(s => s.SessionName.Contains(name));
        }

        public async Task<Sessions?> UpdateAsync(Guid id, Sessions session)
        {
            var existing = await dbContext.Sessions.FirstOrDefaultAsync(s => s.SessionID == id);
            if (existing == null) return null;

            existing.SessionName = session.SessionName;
            existing.SessionStartDate = session.SessionStartDate;
            existing.SessionEndDate = session.SessionEndDate;
            existing.IsActive = session.IsActive;

            await dbContext.SaveChangesAsync();
            return existing;
        }
    }
}
