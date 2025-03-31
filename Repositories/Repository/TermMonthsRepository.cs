using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class TermMonthsRepository : ITermMonthsRepository
    {
        private readonly RozhnInstituteDbContext _dbContext;

        public TermMonthsRepository(RozhnInstituteDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<TermMonths> AddAsync(TermMonths termMonth)
        {
            await _dbContext.TermMonths.AddAsync(termMonth);
            await _dbContext.SaveChangesAsync();
            return termMonth;
        }

        public async Task<TermMonths?> DeleteAsync(Guid id)
        {
            var existingTermMonth = await _dbContext.TermMonths.FindAsync(id);
            if (existingTermMonth == null)
                return null;

            _dbContext.TermMonths.Remove(existingTermMonth);
            await _dbContext.SaveChangesAsync();
            return existingTermMonth;
        }

        public async Task<List<TermMonths>> GetAllAsync()
        {
            return await _dbContext.TermMonths.ToListAsync();
        }

        public async Task<TermMonths?> GetAsync(Guid id)
        {
            return await _dbContext.TermMonths.FindAsync(id);
        }

        public async Task<TermMonths?> UpdateAsync(Guid termMonthID, TermMonths termMonth)
        {
            var existingTermMonth = await _dbContext.TermMonths.FindAsync(termMonthID);
            if (existingTermMonth == null)
                return null;

            existingTermMonth.TermMonth = termMonth.TermMonth;

            await _dbContext.SaveChangesAsync();
            return existingTermMonth;
        }
    }
}