using InstituteWebApp.Models.Domain;

public interface ITeacherRepository
{
    Task<List<Teachers>> GetAllAsync();
    Task<Teachers?> GetByIdAsync(Guid id);
    Task<Teachers?> GetByRegistrationNoAsync(string registrationNo);
    Task<Teachers?> GetByNameAsync(string teacherName);
    Task<Teachers> AddAsync(Teachers teacher);
    Task<Teachers?> UpdateAsync(Guid id, Teachers teacher);
    Task<Teachers?> DeleteAsync(Guid id);
    Task<Teachers?> UpdateStatusAsync(Guid id, bool isTeaching);

}
