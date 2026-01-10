using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ISlotsRepository
    {
        Task<List<Slots>> GetAllAsync();
        Task<Slots?> GetAsync(Guid id);
        Task<Slots> AddAsync(Slots slot);
        Task<Slots?> UpdateAsync(Guid id, Slots slot);
        Task<Slots?> DeleteAsync(Guid id);
    }
}
