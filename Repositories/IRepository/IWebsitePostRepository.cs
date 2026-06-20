using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface IWebsitePostRepository
    {
        Task<List<WebsitePost>> GetAllAsync(string? postType = null, bool publishedOnly = false);
        Task<WebsitePost?> GetAsync(Guid id);
        Task<WebsitePost?> GetBySlugAsync(string slug);
        Task<WebsitePost> AddAsync(WebsitePost post);
        Task<WebsitePost?> UpdateAsync(Guid id, WebsitePost post);
        Task<WebsitePost?> DeleteAsync(Guid id);
    }
}
