using InstituteWebApp.Models.Domain;
using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class TeacherCoursesRepository : ITeacherCoursesRepository
    {
        private readonly RozhnInstituteDbContext dbContext;

        public TeacherCoursesRepository(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<TeacherCourses> AddAsync(TeacherCourses teacherCourse)
        {
            await dbContext.TeacherCourses.AddAsync(teacherCourse);
            await dbContext.SaveChangesAsync();
            return teacherCourse;
        }

        public async Task<TeacherCourses?> DeleteAsync(Guid id)
        {
            var course = await dbContext.TeacherCourses.FindAsync(id);
            if (course == null) return null;

            dbContext.TeacherCourses.Remove(course);
            await dbContext.SaveChangesAsync();
            return course;
        }

        public async Task<List<TeacherCourses>> GetAllAsync()
        {
            return await dbContext.TeacherCourses
                .Include(tc => tc.Teacher)
                .Include(tc => tc.Course)
                .ToListAsync();
        }

        public async Task<TeacherCourses?> GetAsync(Guid id)
        {
            return await dbContext.TeacherCourses
                .Include(tc => tc.Teacher)
                .Include(tc => tc.Course)
                .FirstOrDefaultAsync(tc => tc.TeacherCourseID == id);
        }

        public async Task<List<TeacherCourses>> GetByTeacherAsync(Guid teacherId)
        {
            return await dbContext.TeacherCourses
                .Include(tc => tc.Teacher)
                .Include(tc => tc.Course)
                .Where(tc => tc.TeacherID == teacherId)
                .OrderByDescending(tc => tc.FromDate)
                .ToListAsync();
        }

        public async Task<TeacherCourses?> UpdateAsync(Guid id, TeacherCourses teacherCourse)
        {
            var existing = await dbContext.TeacherCourses.FindAsync(id);
            if (existing == null) return null;

            existing.TeacherID = teacherCourse.TeacherID;
            existing.CourseID = teacherCourse.CourseID;
            existing.CourseIsTaken = teacherCourse.CourseIsTaken;
            existing.FromDate = teacherCourse.FromDate;
            existing.ToDate = teacherCourse.ToDate;

            await dbContext.SaveChangesAsync();
            return existing;
        }
    }
}
