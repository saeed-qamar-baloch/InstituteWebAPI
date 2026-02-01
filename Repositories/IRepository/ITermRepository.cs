using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ITermRepository
    {
        Task<Term> AddAsync(Term term);
        Task<Term?> GetAsync(Guid id);
        Task<Term?> DeleteAsync(Guid id);
        Task<Term?> UpdateAsync(Guid termID,Term term);
        Task<List<Term>> GetAllAsync();
        Task<Term?> GetTermByNameAsync(string TermName);
        Task<Term?> GetActiveAsync();
    }
}
