using AutoMapper;
using InstituteWebAPI.Models.DTO.Lessons;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    /// <summary>Admin authoring of "Learn English" lessons (block-based content).</summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminLessonsController : ControllerBase
    {
        private readonly ILessonRepository lessonRepository;
        private readonly IMapper mapper;

        public AdminLessonsController(ILessonRepository lessonRepository, IMapper mapper)
        {
            this.lessonRepository = lessonRepository;
            this.mapper = mapper;
        }

        // GET api/AdminLessons
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var domain = await lessonRepository.GetAllAsync();
            return Ok(mapper.Map<List<LessonDto>>(domain));
        }

        // GET api/AdminLessons/{id}
        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetByID(Guid id)
        {
            var domain = await lessonRepository.GetAsync(id);
            if (domain == null) return NotFound();
            return Ok(mapper.Map<LessonDto>(domain));
        }

        // POST api/AdminLessons
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddLessonDto addDto)
        {
            if (!IsValidJsonArray(addDto.BlocksJson))
                return BadRequest("BlocksJson must be a valid JSON array.");

            var domain = mapper.Map<Lesson>(addDto);
            domain = await lessonRepository.AddAsync(domain);
            var dto = mapper.Map<LessonDto>(domain);
            return CreatedAtAction(nameof(GetByID), new { id = dto.LessonID }, dto);
        }

        // PUT api/AdminLessons/{id}
        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLessonDto updateDto)
        {
            if (!IsValidJsonArray(updateDto.BlocksJson))
                return BadRequest("BlocksJson must be a valid JSON array.");

            var domain = mapper.Map<Lesson>(updateDto);
            var updated = await lessonRepository.UpdateAsync(id, domain);
            if (updated == null) return NotFound();
            return Ok(mapper.Map<LessonDto>(updated));
        }

        // DELETE api/AdminLessons/{id}
        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await lessonRepository.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<LessonDto>(deleted));
        }

        // POST api/AdminLessons/import?overwrite=true — bulk seed.
        // overwrite=false (default): skip slugs that already exist.
        // overwrite=true: update existing lessons with the same slug (refresh a course pack).
        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] List<AddLessonDto> lessons, [FromQuery] bool overwrite = false)
        {
            if (lessons == null || lessons.Count == 0)
                return BadRequest("No lessons supplied.");

            int imported = 0, updated = 0, skipped = 0;
            var skippedSlugs = new List<string>();
            foreach (var dto in lessons)
            {
                if (string.IsNullOrWhiteSpace(dto.Slug) || !IsValidJsonArray(dto.BlocksJson))
                {
                    skipped++; continue;
                }
                var existing = await lessonRepository.GetBySlugAsync(dto.Slug);
                if (existing != null)
                {
                    if (overwrite)
                    {
                        var upd = mapper.Map<Lesson>(dto);
                        await lessonRepository.UpdateAsync(existing.LessonID, upd);
                        updated++;
                    }
                    else { skipped++; skippedSlugs.Add(dto.Slug); }
                    continue;
                }
                var domain = mapper.Map<Lesson>(dto);
                await lessonRepository.AddAsync(domain);
                imported++;
            }
            return Ok(new { imported, updated, skipped, skippedSlugs });
        }

        private static bool IsValidJsonArray(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                return doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array;
            }
            catch { return false; }
        }
    }
}
