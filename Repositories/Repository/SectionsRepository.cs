using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class SectionsRepository : ISectionsRepository
    {
        private readonly RozhnInstituteDbContext _dbContext;

        public SectionsRepository(RozhnInstituteDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Sections> AddAsync(Sections section)
        {
            await _dbContext.Sections.AddAsync(section);
            await _dbContext.SaveChangesAsync();
            return section;
        }

        public async Task<Sections?> GetAsync(Guid id)
        {
            return await _dbContext.Sections
                .Include(s => s.Course)
                .Include(s => s.term)
                .Include(s => s.Sessions)
                .FirstOrDefaultAsync(s => s.SectionID == id);
        }

        public async Task<Sections?> UpdateAsync(Guid id, Sections section)
        {
            var existing = await _dbContext.Sections.FindAsync(id);
            if (existing == null) return null;

            existing.SectionName = section.SectionName;
            existing.CourseID = section.CourseID;
            existing.TermID = section.TermID;
            existing.SessionID = section.SessionID;
            existing.StartTime = section.StartTime;
            existing.EndTime = section.EndTime;

            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<Sections?> DeleteAsync(Guid id)
        {
            var existing = await _dbContext.Sections.FindAsync(id);
            if (existing == null) return null;

            _dbContext.Sections.Remove(existing);
            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<Sections>> GetAllAsync()
        {
            return await _dbContext.Sections
                .Include(s => s.Course)
                .Include(s => s.term)
                .Include(s => s.Sessions)
                .ToListAsync();
        }
    }
}
