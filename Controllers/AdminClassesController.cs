using AutoMapper;
using InstituteWebAPI.Models.DTO.Classes;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
[Authorize(Roles = "Admin")]
    public class AdminClassesController : ControllerBase
    {
        private readonly IClassesRepository classesRepository;
        private readonly IMapper mapper;

        public AdminClassesController(IClassesRepository classesRepository, IMapper mapper)
        {
            this.classesRepository = classesRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var classes =await classesRepository.GetAllAsync();
             

            return Ok(mapper.Map<List<ClassesDto>>(classes));
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetByID([FromRoute]Guid id)
        {
            var getClass = await classesRepository.GetAsync(id);
            if (getClass == null)
                return NotFound();

            return Ok(mapper.Map<ClassesDto>(getClass));

        }

        [HttpGet]
        [Route("{Name}")]
        public async Task<IActionResult> GetByName(string Name)
        {
            var getClass = await classesRepository.GetByNameAsync(Name);
            if (getClass == null)
                return NotFound();
            return Ok(mapper.Map<ClassesDto>(getClass));
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]AddClassesDto addClassesDto)
        {
            var classDomainModel = mapper.Map<Classes>(addClassesDto);
            classDomainModel = await classesRepository.AddAsync(classDomainModel);

            return Ok(mapper.Map<AddClassesDto>(classDomainModel));
        }
        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute]Guid id, [FromBody] ClassUpdateRequestDto classUpdateRequestDto )
        {
            var classDomainModel = mapper.Map<Classes>(classUpdateRequestDto);
            classDomainModel = await classesRepository.UpdateAsync(id, classDomainModel);

            if (classDomainModel == null) 
                return NotFound();

            return Ok(mapper.Map<ClassesDto>(classDomainModel));
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var deleteClassDomain = await classesRepository.DeleteAsync(id);

            if (deleteClassDomain == null)
                return NotFound();

            return Ok(mapper.Map<ClassesDto>(deleteClassDomain));
        }

        /// <summary>Returns all classes that belong to a specific course.</summary>
        [HttpGet("by-course/{courseId:Guid}")]
        public async Task<IActionResult> GetByCourse([FromRoute] Guid courseId)
        {
            var all = await classesRepository.GetAllAsync();
            var filtered = all.Where(c => c.CourseID == courseId).ToList();
            return Ok(mapper.Map<List<ClassesDto>>(filtered));
        }

    }
}
