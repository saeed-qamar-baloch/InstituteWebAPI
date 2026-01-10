using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class SectionRepository : ISectionRepository
    {
        private readonly RozhnInstituteDbContext dbContext;

        public SectionRepository(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Section> AddAsync(Section section)
        {
            await dbContext.Sections.AddAsync(section);
            await dbContext.SaveChangesAsync();
            return section;
        }

        public async Task<Section?> DeleteAsync(Guid id)
        {
            var existing = await dbContext.Sections.FindAsync(id);
            if (existing == null) return null;

            dbContext.Sections.Remove(existing);
            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<Section>> GetAllAsync()
        {
            return await dbContext.Sections.ToListAsync();
        }

        public async Task<Section?> GetAsync(Guid id)
        {
            return await dbContext.Sections.FirstOrDefaultAsync(s => s.SectionID == id);
        }

        public async Task<Section?> UpdateAsync(Guid id, Section section)
        {
            var existing = await dbContext.Sections.FindAsync(id);
            if (existing == null) return null;

            existing.Name = section.Name;
            await dbContext.SaveChangesAsync();
            return existing;
        }
    }
}
