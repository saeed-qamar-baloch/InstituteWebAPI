using InstituteWebApp.Models.Domain;
using static System.Net.Mime.MediaTypeNames;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ITestsRepository
    {
        Task<Tests> AddAsync(Tests test);
        Task<Tests?> GetAsync(Guid id);
        Task<List<Tests>> GetAllAsync();
        Task<Tests?> UpdateAsync(Guid id, Tests test);
        Task<Tests?> DeleteAsync(Guid id);
        Task<List<Tests>> SearchTestsAsync(string testType, Guid? termMonthID, Guid? currentClassID);
    }
}
