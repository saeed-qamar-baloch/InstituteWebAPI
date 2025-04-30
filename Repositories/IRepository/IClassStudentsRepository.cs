using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface IClassStudentsRepository
    {
        Task<ClassStudents> AddAsync(ClassStudents classStudent);
        Task<ClassStudents?> GetAsync(Guid id);
        Task<List<ClassStudents>> GetAllAsync();
        Task<ClassStudents?> UpdateAsync(Guid id, ClassStudents classStudent);
        Task<ClassStudents?> DeleteAsync(Guid id);
    }
}
