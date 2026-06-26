// StudentsController.cs
using AutoMapper;
using InstituteWebAPI.Models.DTO.Students;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebAPI.Repositories.Repository;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminStudentsController : ControllerBase
    {
        private readonly IStudentRepository studentRepository;
        private readonly IMapper mapper;

        public AdminStudentsController(IStudentRepository studentRepository, IMapper mapper)
        {
            this.studentRepository = studentRepository;
            this.mapper = mapper;
        }

        //api/adminstudents?filterOn=
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? filterOn, [FromQuery] string? filterQuery, [FromQuery] string? sortBy, [FromQuery] bool isAscending, [FromQuery] int pageNumber=1, [FromQuery] int pageSize=100 )
        {
            var students = await studentRepository.GetAllAsync(filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);
            return Ok(mapper.Map<List<StudentDto>>(students));
        }

        [HttpGet("{id:Guid}")]


      
        public async Task<IActionResult> GetById(Guid id)
        {
            var student = await studentRepository.GetByIdAsync(id);
            if (student == null) return NotFound();
            return Ok(mapper.Map<StudentDto>(student));
        }

        [HttpGet("registration/{regNo}")]
        public async Task<IActionResult> GetByRegistrationNo(string regNo)
        {
            var student = await studentRepository.GetByRegistrationNoAsync(regNo);
            if (student == null) return NotFound();
            return Ok(mapper.Map<StudentDto>(student));
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var student = await studentRepository.GetByNameAsync(name);
            if (student == null) return NotFound();
            return Ok(mapper.Map<StudentDto>(student));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromForm] AddStudentDto dto, [FromForm] IFormFile? file)
        {
            
            var domain = mapper.Map<Students>(dto);
            domain.CreatedAt = DateTime.Now;
            domain.ModifiedAt = DateTime.Now;

            domain.file = file;
          
           

            var created = await studentRepository.AddAsync(domain);

            if (created == null)
            {
                return BadRequest(new { errors = new[] { "Student already exists with the same name, father name, and father contact or CNIC." } });
            }
            return Ok(mapper.Map<StudentDto>(created));
        }

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromForm] UpdateStudentDto dto)
        {
            var domain = mapper.Map<Students>(dto);
            domain.file = dto.file;   // must be set BEFORE repository call so photo gets saved

            var updated = await studentRepository.UpdateAsync(id, domain);
            if (updated == null) return NotFound();

            return Ok(mapper.Map<StudentDto>(updated));
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await studentRepository.DeleteAsync(id);
            if (deleted == null) return NotFound();
            return Ok(mapper.Map<StudentDto>(deleted));
        }

        [HttpPatch("{id:Guid}/status")]
        public async Task<IActionResult> UpdateEnrollmentStatus(Guid id, [FromBody] bool isEnrolled)
        {
            var result = await studentRepository.UpdateEnrollmentStatusAsync(id, isEnrolled);
            if (result == null) return NotFound();
            return Ok(mapper.Map<StudentDto>(result));
        }

        [HttpGet("father-name/{fatherName}")]
        public async Task<IActionResult> GetByFatherName(string fatherName)
        {
            var student = await studentRepository.GetByFatherNameAsync(fatherName);
            if (student == null) return NotFound();

            return Ok(mapper.Map<StudentDto>(student));
        }

        [HttpGet("phone/{phone}")]
        public async Task<IActionResult> GetByPhone(string phone)
        {
            var student = await studentRepository.GetByPhoneAsync(phone);
            if (student == null) return NotFound();

            return Ok(mapper.Map<StudentDto>(student));
        }

        [HttpGet("cnic/{cnic}")]
        public async Task<IActionResult> GetByCnic(string cnic)
        {
            var student = await studentRepository.GetByCnicAsync(cnic);
            if (student == null) return NotFound();

            return Ok(mapper.Map<StudentDto>(student));
        }

        /// <summary>
        /// Combined picker search: matches name, registration number, or father name.
        /// Used by the Admissions form student-picker dropdown.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q = "")
        {
            // Searches the full Students table (not a pre-capped page), so a match by
            // name, registration no, or father name is always found regardless of how
            // many students exist or where the match falls alphabetically.
            var students = await studentRepository.SearchAsync(q, 50);
            return Ok(mapper.Map<List<StudentDto>>(students));
        }

    }
}
