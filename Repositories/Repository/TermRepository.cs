using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class TermRepository : ITermRepository
    {
        private readonly RozhnInstituteDbContext _dbContext;

        public TermRepository(RozhnInstituteDbContext dbContext)
        {
            this._dbContext = dbContext;
        }
        public async Task<Term> AddAsync(Term term)
        {

            if (term.IsActive)
            {
                var activeTerms = _dbContext.Term.Where(term => term.IsActive).ToList();

                foreach (var ActiveTerm in activeTerms)
                {
                    ActiveTerm.IsActive = false;
                }
                

            }

            await _dbContext.Term.AddAsync(term);

            await _dbContext.SaveChangesAsync();
            return term;
        }

        public async Task<Term?> DeleteAsync(Guid id)
        {
            var ExistingTerm = await _dbContext.Term.FirstOrDefaultAsync(t => t.TermID == id);
            if (ExistingTerm == null)
                return null;
            _dbContext.Term.Remove(ExistingTerm);
            await _dbContext.SaveChangesAsync();
            return ExistingTerm;

            
        }

        public async Task<List<Term>> GetAllAsync()
        {
            return (await _dbContext.Term.OrderByDescending(t=> t.IsActive).ThenByDescending(t=> t.TermStart).ToListAsync());
        }

        public async Task<Term?> GetAsync(Guid id)
        {
            return await _dbContext.Term.FirstOrDefaultAsync(t=> t.TermID == id);
        }

    

        public async Task<Term?> GetTermByNameAsync(string TermName)
        {
            return await _dbContext.Term.FirstOrDefaultAsync(t => t.TermName.Contains(TermName));
        }


        public async Task<Term?> UpdateAsync(Guid termID, Term term)
        {
            var existingTerm = await _dbContext.Term.FirstOrDefaultAsync(t => t.TermID == termID);
            if (existingTerm == null)
                return null;
            if (term.IsActive)
            {
                var activeTerms = _dbContext.Term.Where(term => term.IsActive).ToList();

                foreach (var ActiveTerm in activeTerms)
                {
                    ActiveTerm.IsActive = false;
                }


            }



            existingTerm.TermID = term.TermID;
            existingTerm.TermName = term.TermName;
            existingTerm.TermStart = term.TermStart;
            existingTerm.TermEnd = term.TermEnd;
            existingTerm.TermDuration = term.TermDuration;
            existingTerm.IsActive = term.IsActive;

            await _dbContext.SaveChangesAsync();
            return existingTerm;
        }
    }
}
