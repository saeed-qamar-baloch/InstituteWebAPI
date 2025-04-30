using InstituteWebApp.Models.Domain;
using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class StudentMarksRepository : IStudentMarksRepository
    {
        private readonly RozhnInstituteDbContext dbContext;

        public StudentMarksRepository(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<StudentMarks> AddAsync(StudentMarks studentMarks)
        {
            await dbContext.StudentMarks.AddAsync(studentMarks);
            await dbContext.SaveChangesAsync();
            return studentMarks;
        }

        public async Task<StudentMarks?> DeleteAsync(Guid id)
        {
            var existing = await dbContext.StudentMarks.FindAsync(id);
            if (existing == null) return null;

            dbContext.StudentMarks.Remove(existing);
            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<StudentMarks>> GetAllAsync()
        {
            return await dbContext.StudentMarks
                .Include(sm => sm.Test)
                .Include(sm => sm.Student)
                .Include(sm => sm.Term)
                .ToListAsync();
        }

        public async Task<StudentMarks?> GetAsync(Guid id)
        {
            return await dbContext.StudentMarks
                .Include(sm => sm.Test)
                .Include(sm => sm.Student)
                .Include(sm => sm.Term)
                .FirstOrDefaultAsync(sm => sm.StudentMarkID == id);
        }

        public async Task<StudentMarks?> UpdateAsync(Guid id, StudentMarks studentMarks)
        {
            var existing = await dbContext.StudentMarks.FindAsync(id);
            if (existing == null) return null;

            existing.ObtainedMarks = studentMarks.ObtainedMarks;
            existing.TestID = studentMarks.TestID;
            existing.StudentID = studentMarks.StudentID;
            existing.TermID = studentMarks.TermID;

            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<StudentMarks>> GetByStudentIdAsync(Guid studentId)
        {
            return await dbContext.StudentMarks
                .Include(sm => sm.Test)
                .Include(sm => sm.Student)
                .Include(sm => sm.Term)
                .Where(sm => sm.StudentID == studentId)
                .ToListAsync();
        }

        public async Task<List<StudentMarks>> GetByTestIdAsync(Guid testId)
        {
            return await dbContext.StudentMarks
                .Include(sm => sm.Test)
                .Include(sm => sm.Student)
                .Include(sm => sm.Term)
                .Where(sm => sm.TestID == testId)
                .ToListAsync();
        }
    }
}
