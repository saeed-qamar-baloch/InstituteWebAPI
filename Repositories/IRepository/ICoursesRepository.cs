using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ICoursesRepository
    {
        Task<Courses> AddAsync(Courses course);
        Task<Courses?> GetAsnyc(Guid id);
        Task<Courses?> DeleteAsync(Guid id);
        Task<Courses?> UpdateAsync(Guid courseID, Courses course);
        Task<List<Courses>> GetAllAsync();
        Task<Courses?> GetCourseByNameAsync(string courseName);
    }
}
