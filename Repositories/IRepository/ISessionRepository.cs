using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ISessionRepository
    {
        Task<Sessions> AddAsync(Sessions session);
        Task<Sessions?> GetAsync(Guid id);
        Task<Sessions?> DeleteAsync(Guid id);
        Task<Sessions?> UpdateAsync(Guid sessionID, Sessions session);
        Task<List<Sessions>> GetAllAsync();
    }
}
