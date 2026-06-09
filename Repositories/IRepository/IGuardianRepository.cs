using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface IGuardianRepository
    {
        /// <summary>Returns all guardians, optionally filtered by studentId.</summary>
        Task<List<Guardian>> GetAllAsync(Guid? studentId = null);

        Task<Guardian?> GetByIdAsync(Guid id);

        /// <summary>Returns all guardians for a specific student.</summary>
        Task<List<Guardian>> GetByStudentIdAsync(Guid studentId);

        Task<Guardian> AddAsync(Guardian guardian);

        Task<Guardian?> UpdateAsync(Guid id, Guardian guardian);

        Task<Guardian?> DeleteAsync(Guid id);
    }
}
