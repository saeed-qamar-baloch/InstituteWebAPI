using InstituteWebAPI.Data;
using InstituteWebApp.Models.Domain;
using InstituteWebAPI.Services.Storage;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;

public class TeacherRepository : ITeacherRepository
{
    private readonly RozhnInstituteDbContext dbContext;
    private readonly ImageStorage imageStorage;
    private readonly IHttpContextAccessor httpContextAccessor;

    public TeacherRepository(RozhnInstituteDbContext dbContext, ImageStorage imageStorage, IHttpContextAccessor httpContextAccessor)
    {
        this.dbContext = dbContext;
        this.imageStorage = imageStorage;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<Teachers> AddAsync(Teachers teacher)
    {



        //Get the current maximum serial number
        int maxSerial = await dbContext.Teachers.MaxAsync(s => (int?)s.Serial) ?? 0;
        int newSerial = maxSerial + 1;

        teacher.Serial = newSerial;

        // Use the provided RegDate to generate the RegistrationNo
        string monthYear = teacher.RegistrationDate.ToString("MMMyy"); // e.g., Jan25
        string formattedSerial = newSerial.ToString("D3");     // e.g., 001
        teacher.RegistrationNo = $"RZST-{monthYear}-{formattedSerial}";


        if (teacher.file != null)
        {
            var fileExtension = Path.GetExtension(teacher.file.FileName);

            var localFilePath = Path.Combine(imageStorage.GetFolder("Teachers"), $"{teacher.RegistrationNo}{fileExtension}");
            var urlPath = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/images/Teachers/{teacher.RegistrationNo}{fileExtension}";

            using var stream = new FileStream(localFilePath, FileMode.Create);
            await teacher.file.CopyToAsync(stream);

            teacher.Picture = urlPath;
        }
        else
        {
            // Picture column is NOT NULL — default to empty string when no photo is uploaded
            teacher.Picture ??= string.Empty;
        }

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


        if (updated.file != null)
        {
            var fileExtension = Path.GetExtension(updated.file.FileName);

            var localFilePath = Path.Combine(imageStorage.GetFolder("Teachers"), $"{teacher.RegistrationNo}{fileExtension}");
            var urlPath = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/images/Teachers/{teacher.RegistrationNo}{fileExtension}";

            using var stream = new FileStream(localFilePath, FileMode.Create);
            await updated.file.CopyToAsync(stream);

            updated.Picture = urlPath;
        }
        else
        {
            // Keep existing picture if no new file uploaded
            updated.Picture = teacher.Picture;
        }

        // Preserve server-managed fields not present on the update DTO
        updated.TeacherID = teacher.TeacherID;   // never let the PK be overwritten
        updated.Serial = teacher.Serial;
        updated.IdentityUserId = teacher.IdentityUserId;
        updated.RegistrationNo = teacher.RegistrationNo;

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
