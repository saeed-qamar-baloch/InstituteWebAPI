using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class SlotsRepository : ISlotsRepository
    {
        private readonly RozhnInstituteDbContext dbContext;

        public SlotsRepository(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Slots> AddAsync(Slots slot)
        {
            await dbContext.Slots.AddAsync(slot);
            await dbContext.SaveChangesAsync();
            return slot;
        }

        public async Task<Slots?> DeleteAsync(Guid id)
        {
            var existing = await dbContext.Slots.FindAsync(id);
            if (existing == null) return null;

            dbContext.Slots.Remove(existing);
            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<Slots>> GetAllAsync()
        {
            return await dbContext.Slots
                .Include(s => s.Course)
                .Include(s => s.Term)
                .Include(s => s.Session)
                .ToListAsync();
        }

        public async Task<Slots?> GetAsync(Guid id)
        {
            return await dbContext.Slots
                .Include(s => s.Course)
                .Include(s => s.Term)
                .Include(s => s.Session)
                .FirstOrDefaultAsync(s => s.SlotID == id);
        }

        public async Task<Slots?> UpdateAsync(Guid id, Slots slot)
        {
            var existing = await dbContext.Slots.FindAsync(id);
            if (existing == null) return null;

            existing.SlotName = slot.SlotName;
            existing.StartTime = slot.StartTime;
            existing.EndTime = slot.EndTime;
            existing.CourseID = slot.CourseID;
            existing.TermID = slot.TermID;
            existing.SessionID = slot.SessionID;

            await dbContext.SaveChangesAsync();
            return existing;
        }
    }
}
