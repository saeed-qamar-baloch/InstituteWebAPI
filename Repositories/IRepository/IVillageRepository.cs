using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface IVillageRepository
    {
        Task<Village> AddAsync(Village village);
        Task<Village?> GetAsync(Guid id);
        Task<Village?> DeleteAsync(Guid id);
        Task<Village?> UpdateAsync(Guid villageID, Village village);
        Task<List<Village>> GetAllAsync();
    }
}
