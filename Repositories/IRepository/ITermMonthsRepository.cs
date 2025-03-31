using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ITermMonthsRepository
    {
        Task<TermMonths> AddAsync(TermMonths termMonth);
        Task<TermMonths?> GetAsync(Guid id);
        Task<TermMonths?> DeleteAsync(Guid id);
        Task<TermMonths?> UpdateAsync(Guid termMonthID, TermMonths termMonth);
        Task<List<TermMonths>> GetAllAsync();
    }
}
