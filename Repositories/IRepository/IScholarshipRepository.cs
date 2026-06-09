using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface IScholarshipRepository
    {
        Task<List<Scholarship>> GetAllAsync(Guid? studentId = null, Guid? admissionId = null, bool activeOnly = false);
        Task<Scholarship?> GetByIdAsync(Guid id);
        Task<List<Scholarship>> GetByStudentIdAsync(Guid studentId);

        /// <summary>
        /// Returns the active scholarship for a given admission and month,
        /// used by the fee generation service to apply discounts.
        /// </summary>
        Task<Scholarship?> GetActiveForMonthAsync(Guid admissionId, DateTime month);

        Task<Scholarship> AddAsync(Scholarship scholarship, string createdByUserId);
        Task<Scholarship?> UpdateAsync(Guid id, Scholarship scholarship);
        Task<Scholarship?> DeleteAsync(Guid id);
    }
}
