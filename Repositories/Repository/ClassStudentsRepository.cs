using InstituteWebApp.Models.Domain;
using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class ClassStudentsRepository : IClassStudentsRepository
    {
        private readonly RozhnInstituteDbContext dbContext;

        public ClassStudentsRepository(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ClassStudents> AddAsync(ClassStudents classStudent)
        {
            await dbContext.ClassStudents.AddAsync(classStudent);
            await dbContext.SaveChangesAsync();
            return classStudent;
        }

        public async Task<ClassStudents?> DeleteAsync(Guid id)
        {
            var existing = await dbContext.ClassStudents.FindAsync(id);
            if (existing == null) return null;

            dbContext.ClassStudents.Remove(existing);
            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<ClassStudents>> GetAllAsync()
        {
            return await dbContext.ClassStudents
                .Include(cs => cs.Student)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Class)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Slot)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Teacher)
                .ToListAsync();
        }

        public async Task<ClassStudents?> GetAsync(Guid id)
        {
            return await dbContext.ClassStudents
                .Include(cs => cs.Student)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Class)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Slot)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Teacher)
                .FirstOrDefaultAsync(cs => cs.ClassStudentID == id);
        }

        public async Task<ClassStudents?> UpdateAsync(Guid id, ClassStudents classStudent)
        {
            var existing = await dbContext.ClassStudents.FindAsync(id);
            if (existing == null) return null;

            existing.StudentID = classStudent.StudentID;
            existing.CurrentClassID = classStudent.CurrentClassID;
            existing.Status = classStudent.Status;

            await dbContext.SaveChangesAsync();
            return existing;
        }
    }
}
