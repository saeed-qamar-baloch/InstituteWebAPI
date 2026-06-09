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
            admission.ModifiedAt = DateTime.Now;

            // Only one active admission per student per course
            if (admission.IsActive)
            {
                var alreadyActive = await dbContext.Admissions
                    .AnyAsync(a => a.StudentID == admission.StudentID && a.CourseID == admission.CourseID && a.IsActive);

                if (alreadyActive)
                {
                    throw new InvalidOperationException("Student already has an active admission in this course");
                }
            }

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
                .Include(a => a.AdmittedClass)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Admissions?> GetAsync(Guid id)
        {
            return await dbContext.Admissions
                .Include(a => a.Student)
                .Include(a => a.Course)
                .Include(a => a.AdmittedClass)
                .FirstOrDefaultAsync(a => a.AdmissionID == id);
        }

        public async Task<Admissions?> UpdateAsync(Guid id, Admissions admission)
        {
            var existing = await dbContext.Admissions.FindAsync(id);
            if (existing == null) return null;

            // If we are setting this admission active, ensure no other active admission exists for the student in the same course
            if (admission.IsActive)
            {
                var alreadyActive = await dbContext.Admissions
                    .AnyAsync(a => a.AdmissionID != id && a.StudentID == existing.StudentID && a.CourseID == admission.CourseID && a.IsActive);

                if (alreadyActive)
                {
                    throw new InvalidOperationException("Student already has an active admission in this course");
                }
            }

            existing.RegistrationDate = admission.RegistrationDate;
            existing.LeavingDate = admission.LeavingDate;
            existing.MonthlyFee = admission.MonthlyFee;
            existing.AdmissionFee = admission.AdmissionFee;
            existing.DueDate = admission.DueDate;
            existing.Status = admission.Status;
            existing.IsActive = admission.IsActive;
            existing.IsFree = admission.IsFree;
            existing.CourseID = admission.CourseID;
            existing.AdmittedClassID = admission.AdmittedClassID;
            existing.ModifiedAt = DateTime.Now;

            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<Admissions>> GetByStudentAsync(Guid studentId)
        {
            return await dbContext.Admissions
                .Include(a => a.Student)
                .Include(a => a.Course)
                .Include(a => a.AdmittedClass)
                .Where(a => a.StudentID == studentId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Admissions>> SearchAdmissionsAsync(string registrationNo, string StudentName, string fatherName, string cnic, string fatherContact)
        {
            return await dbContext.Admissions
                .Include(a => a.Student)
                .Include(a => a.Course)
                .Include(a => a.AdmittedClass)
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
