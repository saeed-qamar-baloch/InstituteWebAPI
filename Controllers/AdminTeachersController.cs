using AutoMapper;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
public class AdminTeachersController : ControllerBase
{
    private readonly ITeacherRepository repo;
    private readonly IMapper mapper;

    public AdminTeachersController(ITeacherRepository repo, IMapper mapper)
    {
        this.repo = repo;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var teachers = await repo.GetAllAsync();
        return Ok(mapper.Map<List<TeacherDto>>(teachers));
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var teacher = await repo.GetByIdAsync(id);
        if (teacher == null) return NotFound();

        return Ok(mapper.Map<TeacherDto>(teacher));
    }

    [HttpGet("registration/{regNo}")]
    public async Task<IActionResult> GetByRegistrationNo(string regNo)
    {
        var teacher = await repo.GetByRegistrationNoAsync(regNo);
        if (teacher == null) return NotFound();

        return Ok(mapper.Map<TeacherDto>(teacher));
    }

    [HttpGet("name/{teacherName}")]
    public async Task<IActionResult> GetByName(string teacherName)
    {
        var teacher = await repo.GetByNameAsync(teacherName);
        if (teacher == null) return NotFound();

        return Ok(mapper.Map<TeacherDto>(teacher));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] AddTeacherDto dto)
    {
        var teacher = mapper.Map<Teachers>(dto);
        teacher.file = dto.file;           // must be set BEFORE AddAsync so the repo can save the photo
        teacher = await repo.AddAsync(teacher);

        return Ok(mapper.Map<TeacherDto>(teacher));
    }

    [HttpPut("{id:Guid}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateTeacherDto dto)
    {
        var teacher = mapper.Map<Teachers>(dto);
        teacher.file = dto.file;
        var updated = await repo.UpdateAsync(id, teacher);
        if (updated == null) return NotFound();

        return Ok(mapper.Map<TeacherDto>(updated));
    }

    [HttpDelete("{id:Guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var deleted = await repo.DeleteAsync(id);
        if (deleted == null) return NotFound();

        return Ok(mapper.Map<TeacherDto>(deleted));
    }

    [HttpPut("updatestatus/{id:Guid}")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] bool isTeaching)
    {
        var updated = await repo.UpdateStatusAsync(id, isTeaching);
        if (updated == null) return NotFound();

        return Ok(mapper.Map<TeacherDto>(updated));
    }
}
