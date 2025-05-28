// StudentRepository.cs
using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace InstituteWebAPI.Repositories.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly RozhnInstituteDbContext _DbContext;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;

        public StudentRepository(RozhnInstituteDbContext _DBContext, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _DbContext = _DBContext;
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;

        }

        public async Task<List<Students>> GetAllAsync(string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool isAscending = true, int pageNumber = 1, int pageSize = 100)
        {

            var students = _DbContext.Students.Include(x => x.Village).AsQueryable();

            //filter
            if (string.IsNullOrWhiteSpace(filterOn) == false && string.IsNullOrWhiteSpace(filterQuery) == false)
            {
                if (filterOn.Equals("Village", StringComparison.OrdinalIgnoreCase))
                {
                    students = students.Where(x => x.Village.VillageName.Contains(filterQuery));
                }
            }

            //sort
            if (string.IsNullOrWhiteSpace(sortBy) == false)
            {
                if (sortBy.Equals("RegDate", StringComparison.OrdinalIgnoreCase))
                {
                    students = isAscending ? students.OrderBy(x => x.RegDate) : students.OrderByDescending(x => x.RegDate);
                }


                else if (sortBy.Equals("IsEnrolled", StringComparison.OrdinalIgnoreCase))
                {
                    students = isAscending ? students.OrderBy(x => x.IsEnrolled) : students.OrderByDescending(x => x.RegDate);
                }
            }

            //Pagination 
            var skipResult = (pageNumber - 1) * pageSize;


            return await students.Skip(skipResult).Take(pageSize).ToListAsync();

            // return await _DbContext.Students.Include(s => s.Village).ToListAsync();
        }

        public async Task<Students?> GetByIdAsync(Guid id)
        {
            return await _DbContext.Students.Include(s => s.Village).FirstOrDefaultAsync(s => s.StudentID == id);
        }

        public async Task<Students?> GetByRegistrationNoAsync(string regNo)
        {
            return await _DbContext.Students.Include(s => s.Village)
                .FirstOrDefaultAsync(s => s.RegistrationNo == regNo);
        }

        public async Task<Students?> GetByNameAsync(string name)
        {
            return await _DbContext.Students.Include(s => s.Village)
                .FirstOrDefaultAsync(s => s.StudentName.Contains(name));
        }

        public async Task<Students> AddAsync(Students student)
        {

            bool StudentExists = _DbContext.Students.Any(x => (x.StudentName == student.StudentName && x.FatherContact == student.FatherContact || student.StudentContact == x.StudentContact)
                || (x.StudentName == student.StudentName && x.FatherCnic == student.FatherCnic)

                );

            if (StudentExists)
            {
                return null;
               
            }


            // Get the current maximum serial number
            int maxSerial = await _DbContext.Students.MaxAsync(s => (int?)s.Serial) ?? 0;
            int newSerial = maxSerial + 1;

            student.Serial = newSerial;

            // Use the provided RegDate to generate the RegistrationNo
            string monthYear = student.RegDate.ToString("MMMyy"); // e.g., Jan25
            string formattedSerial = newSerial.ToString("D3");     // e.g., 001
            student.RegistrationNo = $"RZKG-{monthYear}-{formattedSerial}";

            var fileExtension = Path.GetExtension(student.file.FileName);

            var localFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "Images", "Students", $"{student.RegistrationNo}{fileExtension}");


            using (var image = await Image.LoadAsync(student.file.OpenReadStream()))
            {
                image.Mutate(x=> x.Resize(600,0));

                var jpegEncoder = new JpegEncoder()
                {
                    Quality = 75
                };

                await image.SaveAsync(localFilePath, jpegEncoder);

            }

            //    using var stream = new FileStream(localFilePath, FileMode.Create);

            //await student.file.CopyToAsync(stream);

            var urlFilePath = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/Images/Students/{student.RegistrationNo}{fileExtension}";


            student.CreatedAt = DateTime.Now;
            student.ModifiedAt = DateTime.Now;
            student.Picture = urlFilePath;
            await _DbContext.Students.AddAsync(student);
            await _DbContext.SaveChangesAsync();
            return student;
        }

        public async Task<Students?> UpdateAsync(Guid id, Students student)
        {
            var existing = await _DbContext.Students.FindAsync(id);
            if (existing == null) return null;

            if (student.file != null)
            {
                var fileExtension = Path.GetExtension(student.file.FileName);

                var localFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "Images", "Students", $"{existing.RegistrationNo}{fileExtension}");
                using (var image = await Image.LoadAsync(student.file.OpenReadStream()))
                {
                    image.Mutate(x => x.Resize(600, 0));

                    var jpegEncoder = new JpegEncoder()
                    {
                        Quality = 75
                    };

                    await image.SaveAsync(localFilePath, jpegEncoder);

                }


            

                var UrlFilePath = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}/Images/Students/{existing.RegistrationNo}{fileExtension}";

                existing.Picture = UrlFilePath;
            }

           

            
            existing.StudentName = student.StudentName;
            existing.FatherName = student.FatherName;
            existing.Address = student.Address;
            existing.City = student.City;
            existing.FatherContact = student.FatherContact;
            existing.StudentContact = student.StudentContact;
            existing.Qualification = student.Qualification;
            existing.Institute = student.Institute;
            existing.FatherCnic = student.FatherCnic;
            
            existing.Remarks = student.Remarks;
            existing.ModifiedAt = DateTime.Now;

            await _DbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<Students?> DeleteAsync(Guid id)
        {
            var student = await _DbContext.Students.FindAsync(id);
            if (student == null) return null;

            _DbContext.Students.Remove(student);
            await _DbContext.SaveChangesAsync();
            return student;
        }

        public async Task<Students?> UpdateEnrollmentStatusAsync(Guid id, bool isEnrolled)
        {
            var student = await _DbContext.Students.FindAsync(id);
            if (student == null) return null;

            student.IsEnrolled = isEnrolled;
            student.ModifiedAt = DateTime.Now;
            await _DbContext.SaveChangesAsync();
            return student;
        }



        public async Task<Students?> GetByFatherNameAsync(string fatherName)
        {
            return await _DbContext.Students
                .Include(s => s.Village)
                .FirstOrDefaultAsync(s => s.FatherName.Contains(fatherName));
        }

        public async Task<Students?> GetByPhoneAsync(string phone)
        {
            return await _DbContext.Students
                .Include(s => s.Village)
                .FirstOrDefaultAsync(s => s.FatherContact == phone || s.StudentContact == phone);
        }

        public async Task<Students?> GetByCnicAsync(string cnic)
        {
            return await _DbContext.Students
                .Include(s => s.Village)
                .FirstOrDefaultAsync(s => s.FatherCnic == cnic);
        }
    }
}
