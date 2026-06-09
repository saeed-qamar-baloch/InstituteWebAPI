using InstituteWebApp.Models.Domain;
using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class TestTypeRepository : ITestTypeRepository
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly ITermRepository termRepository;

        public TestTypeRepository(RozhnInstituteDbContext dbContext, ITermRepository termRepository)
        {
            this.dbContext = dbContext;
            this.termRepository = termRepository;
        }

        public async Task<TestType> AddAsync(TestType testType)
        {
            testType.CreatedAt = DateTime.Now;
            testType.ModifiedAt = DateTime.Now;
            await dbContext.TestTypes.AddAsync(testType);
            await dbContext.SaveChangesAsync();
            return testType;
        }

        public async Task<TestType?> DeleteAsync(Guid id)
        {
            var existing = await dbContext.TestTypes.FindAsync(id);
            if (existing == null) return null;
            dbContext.TestTypes.Remove(existing);
            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<TestType>> GetAllAsync()
        {
            // Global test types (TermID == null) plus those scoped to the active term.
            var activeTerm = await termRepository.GetActiveAsync();
            var activeId = activeTerm?.TermID;
            return await dbContext.TestTypes
                .Where(t => t.TermID == null || (activeId != null && t.TermID == activeId))
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<TestType?> GetAsync(Guid id)
        {
            return await dbContext.TestTypes.FindAsync(id);
        }

        public async Task<TestType?> UpdateAsync(Guid id, TestType testType)
        {
            var existing = await dbContext.TestTypes.FindAsync(id);
            if (existing == null) return null;
            existing.Name = testType.Name;
            existing.Description = testType.Description;
            existing.TermID = testType.TermID;
            existing.ModifiedAt = DateTime.Now;
            await dbContext.SaveChangesAsync();
            return existing;
        }
    }
}
