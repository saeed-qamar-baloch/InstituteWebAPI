using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class LessonRepository : ILessonRepository
    {
        private readonly RozhnInstituteDbContext dbContext;

        public LessonRepository(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<Lesson>> GetAllAsync(bool publishedOnly = false)
        {
            var query = dbContext.Lessons.AsQueryable();
            if (publishedOnly)
                query = query.Where(l => l.IsPublished);
            return await query
                .OrderBy(l => l.SectionOrder)
                .ThenBy(l => l.Order)
                .ThenBy(l => l.Title)
                .ToListAsync();
        }

        public async Task<Lesson?> GetAsync(Guid id) =>
            await dbContext.Lessons.FirstOrDefaultAsync(l => l.LessonID == id);

        public async Task<Lesson?> GetBySlugAsync(string slug)
        {
            var normalized = Slugify(slug);
            return await dbContext.Lessons.FirstOrDefaultAsync(l => l.Slug == normalized);
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            var normalized = Slugify(slug);
            return await dbContext.Lessons.AnyAsync(l => l.Slug == normalized);
        }

        public async Task<Lesson> AddAsync(Lesson lesson)
        {
            lesson.LessonID = Guid.NewGuid();
            lesson.CreatedAt = DateTime.UtcNow;
            lesson.UpdatedAt = DateTime.UtcNow;
            lesson.Slug = await ResolveSlugAsync(Slugify(lesson.Slug), null);

            await dbContext.Lessons.AddAsync(lesson);
            await dbContext.SaveChangesAsync();
            return lesson;
        }

        public async Task<Lesson?> UpdateAsync(Guid id, Lesson lesson)
        {
            var existing = await dbContext.Lessons.FirstOrDefaultAsync(l => l.LessonID == id);
            if (existing == null) return null;

            existing.Slug = await ResolveSlugAsync(Slugify(lesson.Slug), id);
            existing.Title = lesson.Title;
            existing.Description = lesson.Description;
            existing.Section = lesson.Section;
            existing.Level = lesson.Level;
            existing.SectionOrder = lesson.SectionOrder;
            existing.Order = lesson.Order;
            existing.BlocksJson = lesson.BlocksJson;
            existing.IsPublished = lesson.IsPublished;
            existing.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<Lesson?> DeleteAsync(Guid id)
        {
            var existing = await dbContext.Lessons.FirstOrDefaultAsync(l => l.LessonID == id);
            if (existing == null) return null;

            dbContext.Lessons.Remove(existing);
            await dbContext.SaveChangesAsync();
            return existing;
        }

        /// <summary>
        /// Lowercases and hyphenates a slug while PRESERVING "/" separators,
        /// so curriculum paths like "grammar/present-simple" stay intact.
        /// </summary>
        private static string Slugify(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var segments = input.Trim().ToLowerInvariant().Split('/', StringSplitOptions.RemoveEmptyEntries);
            var cleaned = segments.Select(seg =>
            {
                var chars = seg.Trim().Select(c => char.IsLetterOrDigit(c) ? c : '-');
                var s = new string(chars.ToArray());
                while (s.Contains("--")) s = s.Replace("--", "-");
                return s.Trim('-');
            });
            return string.Join('/', cleaned.Where(s => s.Length > 0));
        }

        /// <summary>Ensures slug uniqueness with a numeric suffix on the last segment.</summary>
        private async Task<string> ResolveSlugAsync(string baseSlug, Guid? excludeId)
        {
            if (string.IsNullOrEmpty(baseSlug)) baseSlug = "lesson";
            var slug = baseSlug;
            var n = 2;
            while (await dbContext.Lessons.AnyAsync(l =>
                       l.Slug == slug && (excludeId == null || l.LessonID != excludeId)))
            {
                slug = $"{baseSlug}-{n++}";
            }
            return slug;
        }
    }
}
