using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class ScholarshipRepository : IScholarshipRepository
    {
        private readonly RozhnInstituteDbContext _db;

        public ScholarshipRepository(RozhnInstituteDbContext db)
        {
            _db = db;
        }

        private IQueryable<Scholarship> BaseQuery() =>
            _db.Scholarships
               .Include(s => s.Student)
               .Include(s => s.Admission)
               .AsNoTracking();

        public async Task<List<Scholarship>> GetAllAsync(
            Guid? studentId   = null,
            Guid? admissionId = null,
            bool activeOnly   = false)
        {
            var q = BaseQuery();

            if (studentId.HasValue)
                q = q.Where(s => s.StudentID == studentId.Value);

            if (admissionId.HasValue)
                q = q.Where(s => s.AdmissionID == admissionId.Value);

            if (activeOnly)
                q = q.Where(s => s.Status == ScholarshipStatus.Active);

            return await q
                .OrderByDescending(s => s.FromMonth)
                .ToListAsync();
        }

        public async Task<Scholarship?> GetByIdAsync(Guid id) =>
            await BaseQuery().FirstOrDefaultAsync(s => s.ScholarshipID == id);

        public async Task<List<Scholarship>> GetByStudentIdAsync(Guid studentId) =>
            await BaseQuery()
                .Where(s => s.StudentID == studentId)
                .OrderByDescending(s => s.FromMonth)
                .ToListAsync();

        public async Task<Scholarship?> GetActiveForMonthAsync(Guid admissionId, DateTime month)
        {
            // Normalise to first of the month for safe date comparison
            var m = new DateTime(month.Year, month.Month, 1);

            return await BaseQuery()
                .Where(s => s.AdmissionID == admissionId
                         && s.Status      == ScholarshipStatus.Active
                         && s.FromMonth   <= m
                         && s.ToMonth     >= m)
                .FirstOrDefaultAsync();
        }

        public async Task<Scholarship> AddAsync(Scholarship scholarship, string createdByUserId)
        {
            scholarship.ScholarshipID   = Guid.NewGuid();
            scholarship.Status          = ScholarshipStatus.Active;
            scholarship.CreatedByUserID = createdByUserId;
            scholarship.CreatedAt       = DateTime.UtcNow;
            scholarship.ModifiedAt      = DateTime.UtcNow;

            // Normalise dates to first of month
            scholarship.FromMonth = new DateTime(scholarship.FromMonth.Year, scholarship.FromMonth.Month, 1);
            scholarship.ToMonth   = new DateTime(scholarship.ToMonth.Year,   scholarship.ToMonth.Month,   1);

            await _db.Scholarships.AddAsync(scholarship);
            await _db.SaveChangesAsync();

            return (await GetByIdAsync(scholarship.ScholarshipID))!;
        }

        public async Task<Scholarship?> UpdateAsync(Guid id, Scholarship scholarship)
        {
            var existing = await _db.Scholarships.FindAsync(id);
            if (existing == null) return null;

            existing.DiscountPercent = scholarship.DiscountPercent;
            existing.FromMonth       = new DateTime(scholarship.FromMonth.Year, scholarship.FromMonth.Month, 1);
            existing.ToMonth         = new DateTime(scholarship.ToMonth.Year,   scholarship.ToMonth.Month,   1);
            existing.Reason          = scholarship.Reason;
            existing.Status          = scholarship.Status;
            existing.ModifiedAt      = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return (await GetByIdAsync(id))!;
        }

        public async Task<Scholarship?> DeleteAsync(Guid id)
        {
            var existing = await _db.Scholarships.FindAsync(id);
            if (existing == null) return null;

            _db.Scholarships.Remove(existing);
            await _db.SaveChangesAsync();
            return existing;
        }
    }
}
