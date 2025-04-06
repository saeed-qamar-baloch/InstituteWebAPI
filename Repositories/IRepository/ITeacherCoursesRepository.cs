using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ITeacherCoursesRepository
    {
        Task<List<TeacherCourses>> GetAllAsync();
        Task<TeacherCourses?> GetAsync(Guid id);
        Task<TeacherCourses> AddAsync(TeacherCourses teacherCourse);
        Task<TeacherCourses?> UpdateAsync(Guid id, TeacherCourses teacherCourse);
        Task<TeacherCourses?> DeleteAsync(Guid id);
    }
}
