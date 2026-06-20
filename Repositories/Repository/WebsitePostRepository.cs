using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class WebsitePostRepository : IWebsitePostRepository
    {
        private readonly RozhnInstituteDbContext dbContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;

        public WebsitePostRepository(
            RozhnInstituteDbContext dbContext,
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<WebsitePost>> GetAllAsync(string? postType = null, bool publishedOnly = false)
        {
            var query = dbContext.WebsitePosts.AsQueryable();
            if (!string.IsNullOrWhiteSpace(postType))
                query = query.Where(p => p.PostType == postType);
            if (publishedOnly)
                query = query.Where(p => p.IsPublished);
            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<WebsitePost?> GetAsync(Guid id)
        {
            return await dbContext.WebsitePosts.FirstOrDefaultAsync(p => p.WebsitePostID == id);
        }

        public async Task<WebsitePost?> GetBySlugAsync(string slug)
        {
            var normalized = Slugify(slug);
            return await dbContext.WebsitePosts
                .FirstOrDefaultAsync(p => p.PostType == "Page" && p.Slug == normalized);
        }

        public async Task<WebsitePost> AddAsync(WebsitePost post)
        {
            post.WebsitePostID = Guid.NewGuid();
            post.CreatedAt = DateTime.UtcNow;
            post.Slug = await ResolveSlugAsync(post, null);
            post.ImageUrl = await SaveImageAsync(post) ?? string.Empty;

            await dbContext.WebsitePosts.AddAsync(post);
            await dbContext.SaveChangesAsync();
            return post;
        }

        public async Task<WebsitePost?> UpdateAsync(Guid id, WebsitePost post)
        {
            var existing = await dbContext.WebsitePosts.FirstOrDefaultAsync(p => p.WebsitePostID == id);
            if (existing == null)
                return null;

            existing.Title = post.Title;
            existing.Body = post.Body;
            existing.PostType = post.PostType;
            existing.IsPublished = post.IsPublished;
            existing.Slug = await ResolveSlugAsync(post, existing.WebsitePostID);

            if (post.file != null)
            {
                post.WebsitePostID = existing.WebsitePostID;
                existing.ImageUrl = await SaveImageAsync(post) ?? existing.ImageUrl;
            }

            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<WebsitePost?> DeleteAsync(Guid id)
        {
            var existing = await dbContext.WebsitePosts.FirstOrDefaultAsync(p => p.WebsitePostID == id);
            if (existing == null)
                return null;

            dbContext.WebsitePosts.Remove(existing);
            await dbContext.SaveChangesAsync();
            return existing;
        }

        /// <summary>Lower-case, hyphen-separated, URL-safe slug.</summary>
        private static string Slugify(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var chars = input.Trim().ToLowerInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '-');
            var slug = new string(chars.ToArray());
            while (slug.Contains("--")) slug = slug.Replace("--", "-");
            return slug.Trim('-');
        }

        /// <summary>
        /// Pages get a slug (from Slug field or Title) made unique with a numeric
        /// suffix; posts/achievements keep an empty slug.
        /// </summary>
        private async Task<string> ResolveSlugAsync(WebsitePost post, Guid? excludeId)
        {
            if (post.PostType != "Page") return string.Empty;

            var baseSlug = Slugify(string.IsNullOrWhiteSpace(post.Slug) ? post.Title : post.Slug);
            if (baseSlug.Length == 0) baseSlug = "page";

            var slug = baseSlug;
            var n = 2;
            while (await dbContext.WebsitePosts.AnyAsync(p =>
                       p.Slug == slug && (excludeId == null || p.WebsitePostID != excludeId)))
            {
                slug = $"{baseSlug}-{n++}";
            }
            return slug;
        }

        /// <summary>Saves the uploaded file to Images/Website and returns its public URL.</summary>
        private async Task<string?> SaveImageAsync(WebsitePost post)
        {
            if (post.file == null)
                return null;

            var fileExtension = Path.GetExtension(post.file.FileName);
            var fileName = $"{post.WebsitePostID}{fileExtension}";

            var localFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "Images", "Website", fileName);
            var request = httpContextAccessor.HttpContext!.Request;
            var urlPath = $"{request.Scheme}://{request.Host}{request.PathBase}/images/Website/{fileName}";

            using var stream = new FileStream(localFilePath, FileMode.Create);
            await post.file.CopyToAsync(stream);

            return urlPath;
        }
    }
}
