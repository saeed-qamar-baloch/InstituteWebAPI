using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ISectionsRepository
    {
        Task<Sections> AddAsync(Sections section);
        Task<Sections?> GetAsync(Guid id);
        Task<Sections?> UpdateAsync(Guid id, Sections section);
        Task<Sections?> DeleteAsync(Guid id);
        Task<List<Sections>> GetAllAsync();
    }
}
