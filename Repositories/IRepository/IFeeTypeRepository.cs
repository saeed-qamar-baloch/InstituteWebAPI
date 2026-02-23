using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface IFeeTypeRepository
    {
        Task<FeeType> AddAsync(FeeType feeType);
        Task<FeeType?> GetAsync(Guid id);
        Task<List<FeeType>> GetAllAsync();
        Task<FeeType?> UpdateAsync(Guid id, FeeType feeType);
        Task<FeeType?> DeleteAsync(Guid id);
    }
}
