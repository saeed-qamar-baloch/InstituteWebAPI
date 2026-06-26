// IStudentRepository.cs
using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface IStudentRepository
    {
        Task<List<Students>> GetAllAsync(string? filterOn = null, string? filterQuery = null, string? sortBy=null, bool isAscending = true, int pageNumber = 1, int pageSize = 100);
        Task<Students?> GetByIdAsync(Guid id);
        Task<Students?> GetByRegistrationNoAsync(string regNo);
        Task<Students?> GetByNameAsync(string name);
        Task<Students> AddAsync(Students student);
        Task<Students?> UpdateAsync(Guid id, Students student);
        Task<Students?> DeleteAsync(Guid id);
        Task<Students?> UpdateEnrollmentStatusAsync(Guid id, bool isEnrolled);
        Task<Students?> GetByFatherNameAsync(string fatherName);
        Task<Students?> GetByPhoneAsync(string phone);
        Task<Students?> GetByCnicAsync(string cnic);
        Task<List<Students>> SearchAsync(string? q, int limit = 50);
    }
}
