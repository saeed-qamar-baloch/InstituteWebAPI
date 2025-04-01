using AutoMapper;
using InstituteWebAPI.Models.DTO;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminCoursesController : ControllerBase
    {
        private readonly ICoursesRepository coursesRepository;
        private readonly IMapper mapper;

        public AdminCoursesController(ICoursesRepository coursesRepository, IMapper mapper)
        {
            this.coursesRepository = coursesRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courseDomainModel = await coursesRepository.GetAllAsync();
            var courseDto = mapper.Map<List<CourseDto>>(courseDomainModel);
            return Ok(courseDto);
        }
        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetByID(Guid id)
        {
            var CourseDomainModel = await coursesRepository.GetAsnyc(id);

            var CourseDto = mapper.Map<CourseDto>(CourseDomainModel);
            return Ok(CourseDto);

        }

        [HttpGet]
        [Route("{Name}")]

        public async Task<IActionResult> GetByName(string Name)
        {
            var CourseDomainModel = await coursesRepository.GetCourseByNameAsync(Name);
            var courseDto = mapper.Map<CourseDto>(CourseDomainModel);
            return Ok(courseDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddCourseDto addCourseDto)
        {
            var couseDomainModel = mapper.Map<Courses>(addCourseDto);

            couseDomainModel = await coursesRepository.AddAsync(couseDomainModel);

            var courseDto = mapper.Map<AddTermDto>(couseDomainModel);
            return Ok(courseDto);
        }
        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update(Guid id, CourseUpdateRequestDto courseUpdateRequestDto )
        {

            var courseDomainModel = mapper.Map<Courses>(courseUpdateRequestDto);
            courseDomainModel = await coursesRepository.UpdateAsync(id, courseDomainModel);

            if (courseDomainModel == null)
                return NotFound();
            return Ok(mapper.Map<CourseUpdateRequestDto>(courseDomainModel));

        }
        [HttpDelete]
        [Route("{id:Guid}")]

        public async Task<IActionResult> Delete([FromRoute] Guid id
            )
        {

            var deleteCourse = await coursesRepository.DeleteAsync(id);
            if (deleteCourse == null)
                return NotFound();
            return Ok(mapper.Map<CourseDto>(deleteCourse));

        }


    }
}
