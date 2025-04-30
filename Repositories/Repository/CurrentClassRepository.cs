using InstituteWebApp.Models.Domain;
using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class CurrentClassRepository : ICurrentClassRepository
    {
        private readonly RozhnInstituteDbContext dbContext;

        public CurrentClassRepository(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<CurrentClass> AddAsync(CurrentClass currentClass)
        {
            await dbContext.CurrentClasses.AddAsync(currentClass);
            await dbContext.SaveChangesAsync();
            return currentClass;
        }

        public async Task<CurrentClass?> DeleteAsync(Guid id)
        {
            var existing = await dbContext.CurrentClasses.FindAsync(id);
            if (existing == null) return null;

            dbContext.CurrentClasses.Remove(existing);
            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<CurrentClass>> GetAllAsync()
        {
            return await dbContext.CurrentClasses
                .Include(cc => cc.Class)
                .Include(cc => cc.Section)
                .Include(cc => cc.Teacher)
                .Include(cc => cc.Session)
                .Include(cc => cc.Term)
                .ToListAsync();
        }

        public async Task<CurrentClass?> GetAsync(Guid id)
        {
            return await dbContext.CurrentClasses
                .Include(cc => cc.Class)
                .Include(cc => cc.Section)
                .Include(cc => cc.Teacher)
                .Include(cc => cc.Session)
                .Include(cc => cc.Term)
                .FirstOrDefaultAsync(cc => cc.CurrentClassID == id);
        }

        public async Task<CurrentClass?> UpdateAsync(Guid id, CurrentClass currentClass)
        {
            var existing = await dbContext.CurrentClasses.FindAsync(id);
            if (existing == null) return null;

            existing.ClassID = currentClass.ClassID;
            existing.SectionID = currentClass.SectionID;
            existing.TeacherID = currentClass.TeacherID;
            existing.SessionID = currentClass.SessionID;
            existing.TermID = currentClass.TermID;
            existing.IsActive = currentClass.IsActive;

            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<CurrentClass>> SearchCurrentClassesAsync(Guid? classID, Guid? sectionID, Guid? teacherID, Guid? sessionID, Guid? termID, bool? isActive)
        {
            return await dbContext.CurrentClasses
                .Include(cc => cc.Class)
                .Include(cc => cc.Section)
                .Include(cc => cc.Teacher)
                .Include(cc => cc.Session)
                .Include(cc => cc.Term)
                .Where(cc =>
                    (classID == null || cc.ClassID == classID) &&
                    (sectionID == null || cc.SectionID == sectionID) &&
                    (teacherID == null || cc.TeacherID == teacherID) &&
                    (sessionID == null || cc.SessionID == sessionID) &&
                    (termID == null || cc.TermID == termID) &&
                    (isActive == null || cc.IsActive == isActive)
                )
                .ToListAsync();
        }
    }
}
