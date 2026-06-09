using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ITestTypeRepository
    {
        Task<TestType> AddAsync(TestType testType);
        Task<TestType?> GetAsync(Guid id);
        Task<List<TestType>> GetAllAsync();
        Task<TestType?> UpdateAsync(Guid id, TestType testType);
        Task<TestType?> DeleteAsync(Guid id);
    }
}
