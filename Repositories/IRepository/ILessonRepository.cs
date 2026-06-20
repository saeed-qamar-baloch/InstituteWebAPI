using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface ILessonRepository
    {
        Task<List<Lesson>> GetAllAsync(bool publishedOnly = false);
        Task<Lesson?> GetAsync(Guid id);
        Task<Lesson?> GetBySlugAsync(string slug);
        Task<Lesson> AddAsync(Lesson lesson);
        Task<Lesson?> UpdateAsync(Guid id, Lesson lesson);
        Task<Lesson?> DeleteAsync(Guid id);
        Task<bool> SlugExistsAsync(string slug);
    }
}
