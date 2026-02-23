using InstituteWebApp.Models.Domain;
using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class FeeTypeRepository : IFeeTypeRepository
    {
        private readonly RozhnInstituteDbContext dbContext;

        public FeeTypeRepository(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<FeeType> AddAsync(FeeType feeType)
        {
            feeType.CreatedAt = DateTime.Now;
            feeType.ModifiedAt = DateTime.Now;
            await dbContext.FeeTypes.AddAsync(feeType);
            await dbContext.SaveChangesAsync();
            return feeType;
        }

        public async Task<FeeType?> DeleteAsync(Guid id)
        {
            var existing = await dbContext.FeeTypes.FindAsync(id);
            if (existing == null) return null;
            dbContext.FeeTypes.Remove(existing);
            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<FeeType>> GetAllAsync()
        {
            return await dbContext.FeeTypes.ToListAsync();
        }

        public async Task<FeeType?> GetAsync(Guid id)
        {
            return await dbContext.FeeTypes.FindAsync(id);
        }

        public async Task<FeeType?> UpdateAsync(Guid id, FeeType feeType)
        {
            var existing = await dbContext.FeeTypes.FindAsync(id);
            if (existing == null) return null;
            existing.Name = feeType.Name;
            existing.Description = feeType.Description;
            existing.ModifiedAt = DateTime.Now;
            await dbContext.SaveChangesAsync();
            return existing;
        }
    }
}
