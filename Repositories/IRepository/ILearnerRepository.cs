using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ILearnerRepository
    {
        Task<Learner?> GetByIdAsync(Guid id);
        Task<Learner?> GetByEmailAsync(string email);
        Task<Learner?> GetByGoogleSubjectAsync(string sub);
        Task<Learner> AddAsync(Learner learner);
        Task UpdateAsync(Learner learner);
    }
}
