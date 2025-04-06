using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ISessionRepository
    {
        Task<List<Sessions>> GetAllAsync();
        Task<Sessions?> GetAsync(Guid id);
        Task<Sessions?> GetByNameAsync(string name);
        Task<Sessions> AddAsync(Sessions session);
        Task<Sessions?> UpdateAsync(Guid id, Sessions session);
        Task<Sessions?> DeleteAsync(Guid id);
    }
}
