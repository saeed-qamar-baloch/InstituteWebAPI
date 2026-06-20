using AutoMapper;
using InstituteWebAPI.Models.DTO.WebsitePosts;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    /// <summary>Admin management of public-website content (news posts & achievements).</summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminWebsitePostsController : ControllerBase
    {
        private readonly IWebsitePostRepository websitePostRepository;
        private readonly IMapper mapper;

        public AdminWebsitePostsController(IWebsitePostRepository websitePostRepository, IMapper mapper)
        {
            this.websitePostRepository = websitePostRepository;
            this.mapper = mapper;
        }

        // GET api/AdminWebsitePosts?postType=Post|Achievement
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? postType)
        {
            var domain = await websitePostRepository.GetAllAsync(postType);
            return Ok(mapper.Map<List<WebsitePostDto>>(domain));
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetByID(Guid id)
        {
            var domain = await websitePostRepository.GetAsync(id);
            if (domain == null)
                return NotFound();
            return Ok(mapper.Map<WebsitePostDto>(domain));
        }

        // POST api/AdminWebsitePosts  (multipart/form-data, optional image file)
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] AddWebsitePostDto addDto, [FromForm] IFormFile? file)
        {
            var domain = mapper.Map<WebsitePost>(addDto);
            domain.file = file;

            domain = await websitePostRepository.AddAsync(domain);
            var dto = mapper.Map<WebsitePostDto>(domain);
            return CreatedAtAction(nameof(GetByID), new { id = dto.WebsitePostID }, dto);
        }

        // PUT api/AdminWebsitePosts/{id}  (multipart/form-data, optional new image)
        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateWebsitePostDto updateDto, [FromForm] IFormFile? file)
        {
            var domain = mapper.Map<WebsitePost>(updateDto);
            domain.file = file;

            var updated = await websitePostRepository.UpdateAsync(id, domain);
            if (updated == null)
                return NotFound();
            return Ok(mapper.Map<WebsitePostDto>(updated));
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await websitePostRepository.DeleteAsync(id);
            if (deleted == null)
                return NotFound();
            return Ok(mapper.Map<WebsitePostDto>(deleted));
        }
    }
}
