using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class VillageRepository : IVillageRepository
    {
        private readonly RozhnInstituteDbContext _dbContext;

        public VillageRepository(RozhnInstituteDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<Village> AddAsync(Village village)
        {
            await _dbContext.Village.AddAsync(village);
            await _dbContext.SaveChangesAsync();
            return village;
        }

        public async Task<Village?> DeleteAsync(Guid id)
        {
            var existingVillage = await _dbContext.Village.FindAsync(id);
            if (existingVillage == null)
                return null;

            _dbContext.Village.Remove(existingVillage);
            await _dbContext.SaveChangesAsync();
            return existingVillage;
        }

        public async Task<List<Village>> GetAllAsync()
        {
            return await _dbContext.Village.ToListAsync();
        }

        public async Task<Village?> GetAsync(Guid id)
        {
            return await _dbContext.Village.FindAsync(id);
        }

        public async Task<Village?> UpdateAsync(Guid villageID, Village village)
        {
            var existingVillage = await _dbContext.Village.FindAsync(villageID);
            if (existingVillage == null)
                return null;

            existingVillage.VillageName = village.VillageName;

            await _dbContext.SaveChangesAsync();
            return existingVillage;
        }
    }
}
