using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface IAdmissionsRepository
    {
        Task<Admissions> AddAsync(Admissions admission);
        Task<Admissions?> GetAsync(Guid id);
        Task<List<Admissions>> GetAllAsync();
        Task<Admissions?> UpdateAsync(Guid id, Admissions admission);
        Task<Admissions?> DeleteAsync(Guid id);
        Task<List<Admissions>> SearchAdmissionsAsync(string registrationNo, string StudentName, string fatherName, string cnic, string fatherContact);
    }
}
