// StudentRepository.cs
using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly RozhnInstituteDbContext _DbContext;

        public StudentRepository(RozhnInstituteDbContext _DBContext)
        {
            _DbContext = _DBContext;
        }

        public async Task<List<Students>> GetAllAsync()
        {
            return await _DbContext.Students.Include(s => s.Village).ToListAsync();
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
            await _DbContext.Students.AddAsync(student);
            await _DbContext.SaveChangesAsync();
            return student;
        }

        public async Task<Students?> UpdateAsync(Guid id, Students student)
        {
            var existing = await _DbContext.Students.FindAsync(id);
            if (existing == null) return null;

            existing.StudentName = student.StudentName;
            existing.FatherName = student.FatherName;
            existing.Address = student.Address;
            existing.City = student.City;
            existing.FatherContact = student.FatherContact;
            existing.StudentContact = student.StudentContact;
            existing.Qualification = student.Qualification;
            existing.Institute = student.Institute;
            existing.FatherCnic = student.FatherCnic;
            existing.Picture = student.Picture;
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
