using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ICurrentClassRepository
    {
        Task<CurrentClass> AddAsync(CurrentClass currentClass);
        Task<CurrentClass?> GetAsync(Guid id);
        Task<List<CurrentClass>> GetAllAsync();
        Task<CurrentClass?> UpdateAsync(Guid id, CurrentClass currentClass);
        Task<CurrentClass?> DeleteAsync(Guid id);
        Task<List<CurrentClass>> SearchCurrentClassesAsync(Guid? classID, Guid? slotID, Guid? teacherID, Guid? sessionID, Guid? termID, bool? isActive);
    }
}
