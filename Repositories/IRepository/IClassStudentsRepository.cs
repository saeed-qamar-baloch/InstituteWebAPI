using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface IClassStudentsRepository
    {
        Task<ClassStudents> AddAsync(ClassStudents classStudent);
        Task<(int assigned, List<string> skippedNames)> BulkAddAsync(Guid currentClassId, List<Guid> studentIds, string status);
        Task<ClassStudents?> GetAsync(Guid id);
        Task<List<ClassStudents>> GetAllAsync();
        Task<List<ClassStudents>> GetByTermAsync(Guid termId);
        Task<ClassStudents?> UpdateAsync(Guid id, ClassStudents classStudent);
        Task<ClassStudents?> DeleteAsync(Guid id);
        Task<List<ClassStudents>> GetByClassAsync(Guid currentClassId);
        Task<List<Students>> GetUnenrolledStudentsAsync(Guid termId);
    }
}
