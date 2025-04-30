using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface IStudentMarksRepository
    {
        Task<StudentMarks> AddAsync(StudentMarks studentMarks);
        Task<StudentMarks?> GetAsync(Guid id);
        Task<List<StudentMarks>> GetAllAsync();
        Task<StudentMarks?> UpdateAsync(Guid id, StudentMarks studentMarks);
        Task<StudentMarks?> DeleteAsync(Guid id);
        Task<List<StudentMarks>> GetByStudentIdAsync(Guid studentId);
        Task<List<StudentMarks>> GetByTestIdAsync(Guid testId);
    }
}
