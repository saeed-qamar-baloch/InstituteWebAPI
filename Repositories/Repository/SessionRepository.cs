using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class SessionRepository : ISessionRepository
    {
        private readonly RozhnInstituteDbContext _dbContext;

        public SessionRepository(RozhnInstituteDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<Sessions> AddAsync(Sessions session)
        {
            await _dbContext.Sessions.AddAsync(session);
            await _dbContext.SaveChangesAsync();
            return session;
        }

        public async Task<Sessions?> DeleteAsync(Guid id)
        {
            var existingSession = await _dbContext.Sessions.FindAsync(id);
            if (existingSession == null)
                return null;

            _dbContext.Sessions.Remove(existingSession);
            await _dbContext.SaveChangesAsync();
            return existingSession;
        }

        public async Task<List<Sessions>> GetAllAsync()
        {
            return await _dbContext.Sessions.ToListAsync();
        }

        public async Task<Sessions?> GetAsync(Guid id)
        {
            return await _dbContext.Sessions.FindAsync(id);
        }

        public async Task<Sessions?> UpdateAsync(Guid sessionID, Sessions session)
        {
            var existingSession = await _dbContext.Sessions.FindAsync(sessionID);
            if (existingSession == null)
                return null;

            existingSession.SessionName = session.SessionName;
            existingSession.SessionStartDate = session.SessionStartDate;
            existingSession.SessionEndDate = session.SessionEndDate;
            existingSession.IsActive = session.IsActive;

            await _dbContext.SaveChangesAsync();
            return existingSession;
        }
    }
}
