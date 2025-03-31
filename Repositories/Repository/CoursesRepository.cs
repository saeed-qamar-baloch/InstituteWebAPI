using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class CoursesRepository : ICoursesRepository
    {

        private readonly RozhnInstituteDbContext _dbContext;

        public CoursesRepository(RozhnInstituteDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<Courses> AddAsync(Courses course)
        {
            await _dbContext.Courses.AddAsync(course);
            await _dbContext.SaveChangesAsync();
            return course;
        }

        public async Task<Courses?> DeleteAsync(Guid id)
        {
            var existingCourse = await _dbContext.Courses.FirstOrDefaultAsync(c => c.CourseID == id);
            if (existingCourse == null)
                return null;

            _dbContext.Courses.Remove(existingCourse);
            await _dbContext.SaveChangesAsync();
            return existingCourse;
        }

        public async Task<List<Courses>> GetAllAsync()
        {
            return (await _dbContext.Courses.ToListAsync());
        }

        public async Task<Courses?> GetAsnyc(Guid id)
        {
            var existingCourse = await _dbContext.Courses.FirstOrDefaultAsync(c => c.CourseID == id);
            if (existingCourse == null)
                return null;
            return existingCourse;
        }

        public async Task<Courses?> GetCourseByNameAsync(string courseName)
        {
            var existingCourse = await _dbContext.Courses.FirstOrDefaultAsync(c => c.CourseName == courseName);
            if (existingCourse == null)
                return null;
            return existingCourse;

          
        }

        public async Task<Courses?> UpdateAsync(Guid courseID, Courses course)
        {
            var existingCourse = await _dbContext.Courses.FirstOrDefaultAsync(c => c.CourseID == courseID);
            if (existingCourse == null)
                return null;

            existingCourse.CourseID = course.CourseID;
            existingCourse.CourseName = course.CourseName;
            existingCourse.CourseDescription = course.CourseDescription;
            existingCourse.CourseStatus = course.CourseStatus;
            await _dbContext.SaveChangesAsync();
            return existingCourse;

        }
    }
}
