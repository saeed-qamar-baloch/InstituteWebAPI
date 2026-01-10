using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ISectionRepository
    {
        Task<List<Section>> GetAllAsync();
        Task<Section?> GetAsync(Guid id);
        Task<Section> AddAsync(Section section);
        Task<Section?> UpdateAsync(Guid id, Section section);
        Task<Section?> DeleteAsync(Guid id);
    }
}
