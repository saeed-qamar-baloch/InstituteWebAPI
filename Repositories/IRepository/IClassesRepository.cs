using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface IClassesRepository
    {
        Task<Classes> AddAsync(Classes classes);
        Task<Classes?> GetAsync(Guid id);
        Task<Classes?> DeleteAsync(Guid id);
        Task<Classes?> UpdateAsync(Guid classID, Classes classes);
        Task<List<Classes>> GetAllAsync();
        Task<Classes?> GetByNameAsync(string Name);
    }
}
