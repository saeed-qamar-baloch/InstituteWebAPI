using AutoMapper;
using InstituteWebAPI.Models.DTO.WebsitePosts;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    /// <summary>
    /// Anonymous read-only endpoints for the public website (rozhn.org).
    /// NEVER expose student, fee, or user data here.
    /// </summary>
    [Route("api/public")]
    [ApiController]
    [AllowAnonymous]
    public class PublicController : ControllerBase
    {
        private readonly ICoursesRepository coursesRepository;
        private readonly IWebsitePostRepository websitePostRepository;
        private readonly ILessonRepository lessonRepository;
        private readonly InstituteWebAPI.Data.RozhnInstituteDbContext dbContext;
        private readonly IMapper mapper;

        public PublicController(
            ICoursesRepository coursesRepository,
            IWebsitePostRepository websitePostRepository,
            ILessonRepository lessonRepository,
            InstituteWebAPI.Data.RozhnInstituteDbContext dbContext,
            IMapper mapper)
        {
            this.coursesRepository = coursesRepository;
            this.websitePostRepository = websitePostRepository;
            this.lessonRepository = lessonRepository;
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        // GET api/public/content — every CMS content block as { key: object }
        [HttpGet("content")]
        public async Task<IActionResult> GetContent()
        {
            var rows = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .ToListAsync(dbContext.SiteContents);
            var dict = rows.ToDictionary(
                r => r.Key,
                r => System.Text.Json.JsonSerializer.Deserialize<object>(r.Json));
            return Ok(dict);
        }

        // GET api/public/courses — active courses only
        [HttpGet("courses")]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await coursesRepository.GetAllAsync();
            var result = courses
                .Where(c => c.CourseStatus)
                .Select(c => new
                {
                    id = c.CourseID,
                    title = c.CourseName,
                    description = c.CourseDescription,
                });
            return Ok(result);
        }

        // GET api/public/posts — published news posts, newest first
        [HttpGet("posts")]
        public async Task<IActionResult> GetPosts()
        {
            var posts = await websitePostRepository.GetAllAsync("Post", publishedOnly: true);
            return Ok(ToPublicShape(posts));
        }

        // GET api/public/achievements — published achievements, newest first
        [HttpGet("achievements")]
        public async Task<IActionResult> GetAchievements()
        {
            var posts = await websitePostRepository.GetAllAsync("Achievement", publishedOnly: true);
            return Ok(ToPublicShape(posts));
        }

        // GET api/public/pages — published pages (list, no body)
        [HttpGet("pages")]
        public async Task<IActionResult> GetPages()
        {
            var pages = await websitePostRepository.GetAllAsync("Page", publishedOnly: true);
            return Ok(pages.Select(p => new
            {
                id = p.WebsitePostID,
                title = p.Title,
                slug = p.Slug,
                date = p.CreatedAt.ToString("yyyy-MM-dd"),
            }));
        }

        // GET api/public/pages/{slug} — full page content (HTML body)
        [HttpGet("pages/{slug}")]
        public async Task<IActionResult> GetPage(string slug)
        {
            var page = await websitePostRepository.GetBySlugAsync(slug);
            if (page == null || !page.IsPublished)
                return NotFound();
            return Ok(new
            {
                id = page.WebsitePostID,
                title = page.Title,
                slug = page.Slug,
                body = page.Body,
                imageUrl = page.ImageUrl,
                date = page.CreatedAt.ToString("yyyy-MM-dd"),
            });
        }

        // GET api/public/lessons — published lessons (with parsed blocks), curriculum order
        [HttpGet("lessons")]
        public async Task<IActionResult> GetLessons()
        {
            var lessons = await lessonRepository.GetAllAsync(publishedOnly: true);
            return Ok(lessons.Select(ToPublicLesson));
        }

        // GET api/public/lessons/{slug} — single published lesson (slug may contain "/")
        [HttpGet("lessons/{*slug}")]
        public async Task<IActionResult> GetLesson(string slug)
        {
            var lesson = await lessonRepository.GetBySlugAsync(slug);
            if (lesson == null || !lesson.IsPublished)
                return NotFound();
            return Ok(ToPublicLesson(lesson));
        }

        private static object ToPublicLesson(InstituteWebApp.Models.Domain.Lesson l)
        {
            object blocks;
            try { blocks = System.Text.Json.JsonSerializer.Deserialize<object>(l.BlocksJson) ?? new object[0]; }
            catch { blocks = new object[0]; }
            return new
            {
                slug = l.Slug,
                title = l.Title,
                description = l.Description,
                section = l.Section,
                category = l.Category,
                isPopular = l.IsPopular,
                isPractice = l.IsPractice,
                level = l.Level,
                sectionOrder = l.SectionOrder,
                order = l.Order,
                blocks,
                updatedAt = l.UpdatedAt.ToString("yyyy-MM-dd"),
            };
        }

        private static object ToPublicShape(List<InstituteWebApp.Models.Domain.WebsitePost> posts) =>
            posts.Select(p => new
            {
                id = p.WebsitePostID,
                title = p.Title,
                body = p.Body,
                imageUrl = p.ImageUrl,
                date = p.CreatedAt.ToString("yyyy-MM-dd"),
            });
    }
}
