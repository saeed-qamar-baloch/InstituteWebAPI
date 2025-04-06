using InstituteWebAPI.Data;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

public class TeacherRepository : ITeacherRepository
{
    private readonly RozhnInstituteDbContext dbContext;

    public TeacherRepository(RozhnInstituteDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<Teachers> AddAsync(Teachers teacher)
    {
        await dbContext.Teachers.AddAsync(teacher);
        await dbContext.SaveChangesAsync();
        return teacher;
    }

    public async Task<Teachers?> DeleteAsync(Guid id)
    {
        var teacher = await dbContext.Teachers.FindAsync(id);
        if (teacher == null) return null;

        dbContext.Teachers.Remove(teacher);
        await dbContext.SaveChangesAsync();
        return teacher;
    }

    public async Task<List<Teachers>> GetAllAsync()
    {
        return await dbContext.Teachers.ToListAsync();
    }

    public async Task<Teachers?> GetByIdAsync(Guid id)
    {
        return await dbContext.Teachers.FindAsync(id);
    }

    public async Task<Teachers?> GetByRegistrationNoAsync(string registrationNo)
    {
        return await dbContext.Teachers.FirstOrDefaultAsync(t => t.RegistrationNo == registrationNo);
    }

    public async Task<Teachers?> GetByNameAsync(string teacherName)
    {
        return await dbContext.Teachers.FirstOrDefaultAsync(t => t.TeacherName.Contains(teacherName));
    }

    public async Task<Teachers?> UpdateAsync(Guid id, Teachers updated)
    {
        var teacher = await dbContext.Teachers.FindAsync(id);
        if (teacher == null) return null;

        dbContext.Entry(teacher).CurrentValues.SetValues(updated);
        await dbContext.SaveChangesAsync();
        return teacher;
    }

    public async Task<Teachers?> UpdateStatusAsync(Guid id, bool isTeaching)
    {
        var teacher = await dbContext.Teachers.FindAsync(id);
        if (teacher == null) return null;

        teacher.IsTeaching = isTeaching;
        await dbContext.SaveChangesAsync();

        return teacher;
    }
}
