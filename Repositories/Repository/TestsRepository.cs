using InstituteWebApp.Models.Domain;
using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace InstituteWebAPI.Repositories.Repository
{
    public class TestsRepository : ITestsRepository
    {
        private readonly RozhnInstituteDbContext dbContext;

        public TestsRepository(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Tests> AddAsync(Tests test)
        {
            await dbContext.Tests.AddAsync(test);
            await dbContext.SaveChangesAsync();
            return test;
        }

        public async Task<Tests?> DeleteAsync(Guid id)
        {
            var existing = await dbContext.Tests.FindAsync(id);
            if (existing == null) return null;

            dbContext.Tests.Remove(existing);
            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<Tests>> GetAllAsync()
        {
            return await dbContext.Tests
                .Include(t => t.TermMonth)
                .Include(t => t.CurrentClass)
            .ToListAsync();
        }

        public async Task<Tests?> GetAsync(Guid id)
        {
            return await dbContext.Tests
                .Include(t => t.TermMonth)
                .Include(t => t.CurrentClass)
                .FirstOrDefaultAsync(t => t.TestID == id);
        }

        public async Task<Tests?> UpdateAsync(Guid id, Tests test)
        {
            var existing = await dbContext.Tests.FindAsync(id);
            if (existing == null) return null;

            existing.TestType = test.TestType;
            existing.TotalMarks = test.TotalMarks;
            existing.CurrentClassID = test.CurrentClassID;

            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<Tests>> SearchTestsAsync(string testType, Guid? termMonthID, Guid? currentClassID)
        {
            return await dbContext.Tests
                .Include(t => t.TermMonth)
                .Include(t => t.CurrentClass)
                .Where(t =>
                    (testType == null || t.TestType.Contains(testType)) &&
                    (termMonthID == null || t.TermMonthID == termMonthID) &&
                    (currentClassID == null || t.CurrentClassID == currentClassID))
                .ToListAsync();
        }
    }
}
