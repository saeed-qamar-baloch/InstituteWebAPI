using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class ClassesRepository : IClassesRepository
    {
        private readonly RozhnInstituteDbContext _dbContext;

        public ClassesRepository(RozhnInstituteDbContext dbContext)
        {
            this._dbContext = dbContext;

        }
        public async Task<Classes> AddAsync(Classes classes)
        {
          await  _dbContext.Classes.AddAsync(classes);
        await _dbContext.SaveChangesAsync();    
            return classes;
        }

        public async Task<Classes?> DeleteAsync(Guid id)
        {
            var existingClass = await _dbContext.Classes.FirstOrDefaultAsync(c => c.ClassID == id);
            if (existingClass == null)
                return null;

            _dbContext.Remove(existingClass);
            await _dbContext.SaveChangesAsync();
            return existingClass;
        }

        public async Task<List<Classes>> GetAllAsync()
        {
            return await _dbContext.Classes.Include(c=> c.Course).ToListAsync();
        }

        public async Task<Classes?> GetAsync(Guid id)
        {
            var getClass = await _dbContext.Classes.Include(c=> c.Course).FirstOrDefaultAsync(c => c.ClassID == id);
            
            if (getClass == null) 
                return null;
            return getClass;

        }

        public async Task<Classes?> GetByNameAsync(string Name)
        {
            var getClass = await _dbContext.Classes.Include(c => c.Course).FirstOrDefaultAsync(c => c.ClassName.Contains(Name));

            if (getClass == null)
                return null;
            return getClass;
        }

        public async Task<Classes?> UpdateAsync(Guid classID, Classes classes)
        {
            var exisitingClass = await _dbContext.Classes.FirstOrDefaultAsync(c => c.ClassID == classID);
            if (exisitingClass == null)
                return null;

            exisitingClass.ClassName = classes.ClassName;
            exisitingClass.CourseID = classes.CourseID;
            exisitingClass.Rank = classes.Rank;
            await _dbContext.SaveChangesAsync();
            return exisitingClass;
        }
    }
}
