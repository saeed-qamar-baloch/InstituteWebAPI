using InstituteWebApp.Models.Domain;
using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class AdmissionsRepository : IAdmissionsRepository
    {
        private readonly RozhnInstituteDbContext dbContext;

        public AdmissionsRepository(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Admissions> AddAsync(Admissions admission)
        {
            admission.CreatedAt = DateTime.Now;
            
            await dbContext.Admissions.AddAsync(admission);
            await dbContext.SaveChangesAsync();
            return admission;
        }

        public async Task<Admissions?> DeleteAsync(Guid id)
        {
            var existing = await dbContext.Admissions.FindAsync(id);
            if (existing == null) return null;

            dbContext.Admissions.Remove(existing);
            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<Admissions>> GetAllAsync()
        {
            return await dbContext.Admissions
                .Include(a => a.Student)
                .Include(a => a.Course)
                .ToListAsync();
        }

        public async Task<Admissions?> GetAsync(Guid id)
        {
            return await dbContext.Admissions
                .Include(a => a.Student)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AdmissionID == id);
        }

        public async Task<Admissions?> UpdateAsync(Guid id, Admissions admission)
        {
            var existing = await dbContext.Admissions.FindAsync(id);
            if (existing == null) return null;

            existing.RegistrationDate = admission.RegistrationDate;
            existing.LeavingDate = admission.LeavingDate;
            existing.Status = admission.Status;
            existing.IsActive = admission.IsActive;
            existing.CourseID = admission.CourseID;
            existing.ModifiedAt = DateTime.Now;

            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<Admissions>> SearchAdmissionsAsync(string registrationNo, string StudentName, string fatherName, string cnic, string fatherContact)
        {
            return await dbContext.Admissions
                .Include(a => a.Student)
                .Include(a => a.Course)
                .Where(a =>
                    (registrationNo == null || a.Student.RegistrationNo.Contains(registrationNo)) &&
                    (StudentName == null || a.Student.StudentName.Contains(StudentName)) &&
                    (fatherName == null || a.Student.FatherName.Contains(fatherName)) &&
                    (cnic == null || a.Student.FatherCnic.Contains(cnic)) &&
                    (fatherContact == null || a.Student.FatherContact.Contains(fatherContact)))
                .ToListAsync();
        }
    }
}
